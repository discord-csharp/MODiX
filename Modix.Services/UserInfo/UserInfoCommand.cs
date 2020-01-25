#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modix.Data;
using Modix.Data.Extensions;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.Extensions;
using Modix.Services.Images;
using Modix.Services.Utilities;

namespace Modix.Services.UserInfo
{
    public class UserInfoCommand : IRequest
    {
        public UserInfoCommand(IMessage message, ulong guildId, ulong userId)
        {
            Message = message;
            GuildId = guildId;
            UserId = userId;
        }
        public ulong UserId { get; }
        public ulong GuildId { get; }
        public IMessage Message { get; }
    }

    public class UserInfoCommandHandler : AsyncRequestHandler<UserInfoCommand>
    {
        private readonly ModixContext _modixContext;
        private readonly IDiscordClient _discordClient;
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;
        private readonly IImageService _imageService;
        private readonly ILogger<UserInfoCommandHandler> _logger;
        private readonly ModixConfig _modixConfig;

        public UserInfoCommandHandler(ModixContext modixContext, IDiscordClient discordClient,
            IAutoRemoveMessageService autoRemoveMessageService, IImageService imageService, ILogger<UserInfoCommandHandler> logger,
            IOptions<ModixConfig> modixOptions)
        {
            _modixContext = modixContext;
            _discordClient = discordClient;
            _autoRemoveMessageService = autoRemoveMessageService;
            _imageService = imageService;
            _logger = logger;
            _modixConfig = modixOptions.Value;
        }

        protected override async Task Handle(UserInfoCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            _logger.LogDebug("Getting info for user {UserId} in guild {GuildId}", request.UserId, request.GuildId);

            var guild = await _discordClient.GetGuildAsync(request.GuildId, options: new RequestOptions
            {
                CancelToken = cancellationToken,
            });

            var discordUser = await guild.GetUserAsync(request.UserId);

            _logger.LogDebug("Got user {UserId} from Discord API", request.UserId);

            if (discordUser is null)
            {
                // If we have nothing from Discord or from the database,
                // we'll need to assume this user doesn't exist

                _logger.LogDebug("Discord user {UserId} is null, cannot return info, replying with retrieval error embed", request.UserId);

                var embed = new EmbedBuilder()
                    .WithTitle("Retrieval Error")
                    .WithColor(Color.Red)
                    .WithDescription("Sorry, we don't have any data for that user - and we couldn't find any, either.")
                    .AddField("User Id", request.UserId);

                await _autoRemoveMessageService.RegisterRemovableMessageAsync(
                    request.Message.Author,
                    embed,
                    async (builder) =>
                        await request.Message.ReplyAsync(string.Empty, embed: builder.Build()));

                stopwatch.Stop();

                return;
            }

            var modixUser = await GetUserAsync(request.UserId, request.GuildId, cancellationToken);

            var userInfoBuilder = UserInfoEmbedBuilderHelper.Create();

            userInfoBuilder
                .WithId(request.UserId)
                .WithClickableMention(request.UserId)
                .WithStatus(discordUser.Status)
                .WithFirstAndLastSeen(modixUser.FirstSeen, modixUser.LastSeen);

            if (modixUser.Infractions.Any(x => x.Type == InfractionType.Ban))
            {
                var banReason = modixUser
                    .Infractions
                    .Where(x => x.Type == InfractionType.Ban)
                    .OrderByDescending(x => x.Created)
                    .Select(x => x.Reason)
                    .First();

                userInfoBuilder.WithBan(banReason);
            }

            var participation = await GetParticipationStatisticsAsync(guild, request.UserId, cancellationToken);

            if (participation is { })
            {
                userInfoBuilder
                    .WithParticipationInPeriod(participation.UserRank,
                        participation.TotalNumberOfMessagesInPeriod,
                        participation.TotalNumberOfMessagesIn7Days,
                        participation.AverageMessagesSent,
                        participation.Percentile);

                if (participation.TopChannelParticipation is { })
                {
                    userInfoBuilder.WithChannel(participation.TopChannelParticipation.ChannelId,
                        participation.TopChannelParticipation.NumberOfMessages);
                }
            }

            userInfoBuilder
                .WithMemberInformation(discordUser);

            var promotionHistory = modixUser
                .Promotions
                .Select(promotion => (promotion.TargetRoleId, promotion.ClosedDate))
                .ToList();

            if (promotionHistory.Any())
            {
                userInfoBuilder.WithPromotions(promotionHistory);
            }

            var infractionsDictionary = modixUser.Infractions
                .GroupBy(x => x.Type)
                .ToDictionary(d => d.Key, dtos => dtos.Count());

            userInfoBuilder.WithInfractions(_modixConfig.WebsiteBaseUrl, request.UserId, (IGuildChannel)request.Message.Channel,
                infractionsDictionary);

            var content = userInfoBuilder.Build();

            var avatar = discordUser.GetDefiniteAvatarUrl();

            var colorTask = await _imageService.GetDominantColorAsync(new Uri(avatar));

            stopwatch.Stop();

            var embedBuilder = new EmbedBuilder()
                .WithUserAsAuthor(discordUser)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithThumbnailUrl(avatar)
                .WithDescription(content)
                .WithColor(colorTask)
                .WithFooter($"Completed after {stopwatch.ElapsedMilliseconds} ms");

            embedBuilder.Author.IconUrl = avatar;

            var usersAbleToRemove = new[]
            {
                discordUser,
                request.Message.Author,
            };

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(
                usersAbleToRemove,
                embedBuilder,
                async (builder) => await request.Message.ReplyAsync(string.Empty, builder.Build()));
        }

        private async Task<UserInfoUserDto?> GetUserAsync(ulong userId, ulong guildId, CancellationToken cancellationToken)
        {
            return await _modixContext
                .GuildUsers
                .WhereUserInGuild(userId, guildId)
                .Select(x => new UserInfoUserDto
                {
                    FirstSeen = x.FirstSeen,
                    LastSeen = x.LastSeen,
                    Infractions = x.Infractions
                        .AsQueryable()
                        .Where(InfractionQueryExtensions.IsActive())
                        .Select(d => new UserInfoUserDto.UserInfoInfractionDto
                        {
                            Type = d.Type,
                            Created = d.CreateAction.Created,
                            Reason = d.Reason,
                        }).ToList(),
                    Promotions = x.PromotionCampaigns
                        .AsQueryable()
                        .Where(PromotionCampaignQueryExtensions.IsAccepted())
                        .Select(d => new UserInfoUserDto.UserInfoPromotionCampaignDto
                        {
                            ClosedDate = d.CloseAction!.Created,
                            TargetRoleId = d.TargetRoleId,
                        }).ToList(),
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<UserInfoParticipationDto?> GetParticipationStatisticsAsync(IGuild guild, ulong userId, CancellationToken cancellationToken)
        {
            const int NumberOfDaysInPeriodToCount = 30;
            var now = DateTimeOffset.UtcNow;
            var startingThreshold = now.AddDays(-NumberOfDaysInPeriodToCount);
            var weekThreshold = now.AddDays(-7);

            var messages = await _modixContext
                .Messages
                .WhereIsFromGuildUser(userId, guild.Id)
                .WhereCountsTowardsParticipation()
                .Where(x => x.Timestamp > startingThreshold)
                .GroupBy(x => new { x.Timestamp.ToUniversalTime().Date, x.ChannelId })
                .Select(x => new
                {
                    Date = x.Key.Date,
                    ChannelId = x.Key.ChannelId,
                    NumberOfMessages = x.Count(),
                }).ToListAsync(cancellationToken: cancellationToken);

            if (!messages.Any())
            {
                return null;
            }

            var rankedUsers = await _modixContext
                .GuildUsers
                .Where(x => x.GuildId == guild.Id)
                .Where(x => x.Messages
                    .AsQueryable()
                    .Where(MessageQueryExtensions.CountsTowardsParticipation())
                    .Any(d => d.Timestamp > startingThreshold))
                .Select(x =>
                    new
                    {
                        x.UserId,
                        NumberOfMessages = x.Messages
                            .AsQueryable()
                            .Where(MessageQueryExtensions.CountsTowardsParticipation())
                            .Count(d => d.Timestamp > startingThreshold),
                    })
                .OrderByDescending(x => x.NumberOfMessages)
                .ToListAsync(cancellationToken: cancellationToken);

            var totalNumberOfMessagesInPeriod = messages.Sum(x => x.NumberOfMessages);
            var totalNumberOfMessagesIn7Days = messages.Where(x => x.Date.Date > weekThreshold.Date).Sum(d => d.NumberOfMessages);
            var averageMessagesSent = Math.Round((double)totalNumberOfMessagesInPeriod / NumberOfDaysInPeriodToCount, 2, MidpointRounding.ToZero);

            var userRank = 1;
            double percentile = 1;

            // If we have more than one ranked user, show rank, otherwise we'll
            // default to having the user that is being called info on, be the
            // top user
            if (rankedUsers.Count > 1)
            {
                var rankedUser = rankedUsers.Single(x => x.UserId == userId);
                userRank = rankedUsers.IndexOf(rankedUser) + 1;
                percentile = (1 - (userRank / rankedUsers.Count)) * 100;
            }

            var participation = new UserInfoParticipationDto(userRank, totalNumberOfMessagesInPeriod,
                totalNumberOfMessagesIn7Days, averageMessagesSent, percentile);

            if (messages.Any())
            {
                var mostPopularChannel = messages
                    .OrderByDescending(x => x.NumberOfMessages)
                    .Single();

                var channel = await guild.GetChannelAsync(mostPopularChannel.ChannelId);

                if (channel.IsPublic())
                {
                    participation.TopChannelParticipation = new UserInfoParticipationDto.UserInfoParticipationTopChannelDto(mostPopularChannel.ChannelId,
                        mostPopularChannel.NumberOfMessages);
                }
            }

            return participation;
        }
    }
}
