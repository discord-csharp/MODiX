#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Humanizer;
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
        private readonly LappingStopwatch _lappingStopwatch = new LappingStopwatch();

        public UserInfoCommandHandler(ModixContext modixContext, IDiscordClient discordClient,
            IAutoRemoveMessageService autoRemoveMessageService, IImageService imageService,
            ILogger<UserInfoCommandHandler> logger, IOptions<ModixConfig> modixOptions)
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
            var embedId = Guid.NewGuid().ToString("n");

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            _lappingStopwatch.Start();

            _logger.LogDebug("Getting info for user {UserId} in guild {GuildId}", request.UserId, request.GuildId);

            var guild = await _discordClient.GetGuildAsync(request.GuildId, options: new RequestOptions
            {
                CancelToken = cancellationToken,
            });

            _lappingStopwatch.Lap("Get guild");

            var discordUser = await guild.GetUserAsync(request.UserId);

            _lappingStopwatch.Lap("Get guild user");

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
                _lappingStopwatch.Stop();

                return;
            }

            var modixUser = await GetUserAsync(request.UserId, request.GuildId, cancellationToken);

            _lappingStopwatch.Lap("Get MODiX user");

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

            _lappingStopwatch.Lap("Initial embed build");

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

            _lappingStopwatch.Lap("Top channel");

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

            _lappingStopwatch.Lap("Build embed content");

            var avatar = discordUser.GetDefiniteAvatarUrl();

            var colorTask = await _imageService.GetDominantColorAsync(new Uri(avatar));

            _lappingStopwatch.Lap("Dominant colour");

            stopwatch.Stop();

            var embedBuilder = new EmbedBuilder()
                .WithUserAsAuthor(discordUser)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithThumbnailUrl(avatar)
                .WithDescription(content)
                .WithColor(colorTask)
                .WithFooter($"Completed after {stopwatch.ElapsedMilliseconds} ms ({embedId})");

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

            _lappingStopwatch.Lap("Send message");

            _lappingStopwatch.Stop();

            var lapsDump = string.Join("\n", _lappingStopwatch.Laps.Select(x => $"{x.LapName} {x.Elapsed.TotalMilliseconds:0.####}ms\n----------"));

            var statsEmbed = new EmbedBuilder()
                .WithTitle($"Perf dump for {embedId}")
                .WithDescription(lapsDump);

            await request.Message.ReplyAsync(string.Empty, statsEmbed.Build());
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
                .WhereIsUserInGuild(userId, guild.Id)
                .WhereCountsTowardsParticipation()
                .Where(x => x.Timestamp > startingThreshold)
                .GroupBy(x => new { x.Timestamp.ToUniversalTime().Date, x.ChannelId })
                .Select(x => new
                {
                    Date = x.Key.Date,
                    ChannelId = x.Key.ChannelId,
                    NumberOfMessages = x.Count(),
                }).ToListAsync(cancellationToken: cancellationToken);

            _lappingStopwatch.Lap("Messages from MODiX db");

            if (!messages.Any())
            {
                return null;
            }

            var numberOfMessagesIn7DayPeriod = messages.Where(x => x.Date > weekThreshold).Sum(d => d.NumberOfMessages);
            var participationForUser = await GetGuildUserParticipationStatistics(guild.Id, userId, cancellationToken);

            var participation = new UserInfoParticipationDto(participationForUser.Rank, messages.Sum(d => d.NumberOfMessages),
                numberOfMessagesIn7DayPeriod, participationForUser.AveragePerDay, participationForUser.Percentile);

            if (messages.Any())
            {
                var mostPopularChannel = messages
                    .GroupBy(x => x.ChannelId)
                    .OrderByDescending(x => x.Sum(d => d.NumberOfMessages))
                    .First();

                var channel = await guild.GetChannelAsync(mostPopularChannel.Key);

                if (channel.IsPublic())
                {
                    participation.TopChannelParticipation = new UserInfoParticipationDto.UserInfoParticipationTopChannelDto(mostPopularChannel.Key,
                        mostPopularChannel.Sum(d => d.NumberOfMessages));
                }
            }

            return participation;
        }

        private async Task<GuildUserParticipationStatistics> GetGuildUserParticipationStatistics(ulong guildId, ulong userId, CancellationToken cancellationToken)
        {
            const int NumberOfDays = 30;

            var minimumTime = DateTimeOffset.UtcNow - TimeSpan.FromDays(NumberOfDays);

            var groupedMessages = _modixContext.Messages
                .Where(x => x.Channel.DesignatedChannelMappings
                    .Any(x => x.Type == DesignatedChannelType.CountsTowardsParticipation))
                .Where(x => x.GuildId == guildId)
                .Where(x => x.Timestamp >= minimumTime)
                .GroupBy(x => x.AuthorId)
                .Select(x => new
                {
                    AuthorId = x.Key,
                    Count = x.Count(),
                    AveragePerDay = x.Count() / NumberOfDays
                })
                .OrderByDescending(x => x.AveragePerDay);

            var totalCount = await groupedMessages.SumAsync(x => x.Count, cancellationToken);

            _lappingStopwatch.Lap("Participation summed");

            var thisUser = await groupedMessages.SingleOrDefaultAsync(x => x.AuthorId == userId, cancellationToken);

            _lappingStopwatch.Lap("Participation user queried");

            var percentile = thisUser.Count / totalCount * 100;

            // rank = number of users with an average per day higher than this
            // user plus one as it is 1-based rather than 0-based
            var rank = await groupedMessages
                .Where(x => x.AveragePerDay > thisUser.AveragePerDay)
                .CountAsync(cancellationToken) + 1;

            _lappingStopwatch.Lap("Participation rank counted");

            return new GuildUserParticipationStatistics
            {
                GuildId = guildId,
                UserId = userId,

                Rank = rank,
                AveragePerDay = thisUser.AveragePerDay,
                Percentile = percentile
            };
        }
    }
}
