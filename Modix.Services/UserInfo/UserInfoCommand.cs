#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.Extensions;
using Modix.Services.Utilities;

namespace Modix.Services.UserInfo
{
    public class UserInfoCommand : IRequest
    {
        public UserInfoCommand(IMessage message, ulong userId)
        {
            Message = message;
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

        public UserInfoCommandHandler(ModixContext modixContext, IDiscordClient discordClient,
            IAutoRemoveMessageService autoRemoveMessageService)
        {
            _modixContext = modixContext;
            _discordClient = discordClient;
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        protected override async Task Handle(UserInfoCommand request,
            CancellationToken cancellationToken)
        {
            var guild = await _discordClient.GetGuildAsync(request.GuildId);
            var discordUser = await _discordClient.GetUserAsync(request.UserId);

            var modixUser = await _modixContext
                .GuildUsers
                .Where(x => x.UserId == request.UserId)
                .Where(x => x.GuildId == request.GuildId)
                .Select(x => new
                {
                    x.FirstSeen,
                    x.LastSeen,
                    Infractions = x.Infractions.Select(d => new
                    {

                    }).ToList(),
                })
                .FirstOrDefaultAsync();

            if (discordUser is null
                && modixUser is null)
            {
                // If we have nothing from Discord or from the database,
                // we'll need to assume this user doesn't exist

                var embed = new EmbedBuilder()
                    .WithTitle("Retrieval Error")
                    .WithColor(Color.Red)
                    .WithDescription("Sorry, we don't have any data for that user - and we couldn't find any, either.")
                    .AddField("User Id", request.UserId);

                await _autoRemoveMessageService.RegisterRemovableMessageAsync(
                    request.Message.Author,
                    embed,
                    async (embedBuilder) =>
                        await request.Message.ReplyAsync(string.Empty, embed: embedBuilder.Build()));
                return;
            }

            const int NumberOfDaysInPeriodToCount = 30;
            var startingThreshold = DateTimeOffset.UtcNow.AddDays(-NumberOfDaysInPeriodToCount);

            var messages = await _modixContext
                .Messages
                .Where(x => x.GuildId == request.GuildId)
                .Where(x => x.AuthorId == request.UserId)
                .Where(x => x.Channel.DesignatedChannelMappings.Any(d =>
                    d.Type == DesignatedChannelType.CountsTowardsParticipation))
                .Where(x => x.Timestamp > startingThreshold)
                .GroupBy(x => x.Timestamp.Date)
                .Select(x => new
                {
                    Date = x.Key,
                    NumberOfMessages = x.Count()
                }).ToListAsync(cancellationToken: cancellationToken);

            var rankedUsers = await _modixContext
                .GuildUsers
                .Where(x => x.Messages
                    .Where(f => f.Channel.DesignatedChannelMappings.Any(d =>
                    d.Type == DesignatedChannelType.CountsTowardsParticipation)).Any(d => d.Timestamp > startingThreshold))
                .Where(x => x.GuildId == request.GuildId)
                .Select(x =>
                new
                {
                    x.UserId,
                    NumberOfMessages = x.Messages.Where(e => e.Channel.DesignatedChannelMappings.Any(d =>
                        d.Type == DesignatedChannelType.CountsTowardsParticipation)).Count(d => d.Timestamp > startingThreshold),
                })
                .OrderByDescending(x => x.NumberOfMessages)
                .ToListAsync(cancellationToken: cancellationToken);

            var totalNumberOfMessagesInPeriod = messages.Sum(x => x.NumberOfMessages);
            var averageMessagesSent = messages.Sum(x => x.NumberOfMessages) / NumberOfDaysInPeriodToCount;

            var rankedUser = rankedUsers.Single(x => x.UserId == request.UserId);
            var userRank = rankedUsers.IndexOf(rankedUser) + 1;


        }
    }

    public class UserInfoEmbedBuilderHelper
    {
        private readonly StringBuilder _content = new StringBuilder();
        private readonly DateTimeOffset _nowUtc = DateTimeOffset.UtcNow;

        public static UserInfoEmbedBuilderHelper Create()
        {
            return new UserInfoEmbedBuilderHelper();
        }

        private UserInfoEmbedBuilderHelper()
        {
            _content.AppendLine("**\u276F User Information**");
        }

        public UserInfoEmbedBuilderHelper WithId(ulong userId)
        {
            _content.AppendLine("ID: " + userId);
            return this;
        }

        public UserInfoEmbedBuilderHelper WithClickableMention(ulong userId)
        {
            _content.AppendLine("Profile: " + MentionUtils.MentionUser(userId));
            return this;
        }

        public UserInfoEmbedBuilderHelper WithFirstAndLastSeen(DateTimeOffset firstSeen, DateTimeOffset lastSeen)
        {
            _content.AppendLine($"First Seen: {FormatUtilities.FormatTimeAgo(_nowUtc, firstSeen)}");
            _content.AppendLine($"Last Seen: {FormatUtilities.FormatTimeAgo(_nowUtc, lastSeen)}");

            return this;
        }

        public UserInfoEmbedBuilderHelper WithStatus(UserStatus status)
        {
            _content.AppendLine($"Status: {status.Humanize()}");
            return this;
        }

        public UserInfoEmbedBuilderHelper WithBan(string reason)
        {
            _content.AppendLine($"🔨\\ **Banned**: {reason}");
            return this;
        }

        public UserInfoEmbedBuilderHelper WithParticipationInPeriod(int rank,
            int numberOfMessagesIn30Days,
            int numberOfMessagesIn7Days,
            double averagePerDay)
        {
            _content.AppendLine("**\u276F Guild Participation**");
            _content.AppendFormat("Rank: {0} {1}\n", rank, GetParticipationEmoji(rank));
            _content.AppendFormat($"Last 7 days: {numberOfMessagesIn7Days}");
            _content.AppendFormat($"Last 30 days: {numberOfMessagesIn30Days}");
            _content.AppendFormat($"Average/day: {averagePerDay}");

            return this;
        }

        private string GetParticipationEmoji(int rank)
        {
            return rank switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => "🏆",
            };
        }
    }

    public class UserInfoUserDto
    {
        public ulong UserId { get; }
        public string Name { get; }
    }
}
