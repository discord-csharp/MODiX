using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Utilities;

namespace Modix.Controllers
{
    [Route("~/api/userInformation")]
    public class UserInformationController : ModixController
    {
        private IUserService UserService { get; }

        private IMessageRepository MessageRepository { get; }

        public UserInformationController(
            IDiscordSocketClient client,
            IAuthorizationService modixAuth,
            IUserService userService,
            IMessageRepository messageRepository)
            : base(client, modixAuth)
        {
            UserService = userService;
            MessageRepository = messageRepository;
        }

        [HttpGet("{userIdString}")]
        public async Task<IActionResult> GetUserInformationAsync(string userIdString)
        {
            if (!ulong.TryParse(userIdString, out var userId))
                return Ok(null);

            var userInformation = await UserService.GetUserInformationAsync(UserGuild.Id, userId);

            if (userInformation is null)
                return Ok(null);

            var userRank = await MessageRepository.GetGuildUserParticipationStatistics(UserGuild.Id, userId);
            var messages7 = await MessageRepository.GetGuildUserMessageCountByDate(UserGuild.Id, userId, TimeSpan.FromDays(7));
            var messages30 = await MessageRepository.GetGuildUserMessageCountByDate(UserGuild.Id, userId, TimeSpan.FromDays(30));

            var roles = userInformation.RoleIds
                .Select(x => UserGuild.GetRole(x))
                .OrderByDescending(x => x.IsHoisted)
                .ThenByDescending(x => x.Position)
                .ToArray();

            var mapped = new
            {
                Id = userInformation.Id.ToString(),
                Username = userInformation.Username,
                Nickname = userInformation.Nickname,
                Discriminator = userInformation.Discriminator,
                AvatarUrl = userInformation.AvatarId != null ? userInformation.GetAvatarUrl(ImageFormat.Auto, 256) : userInformation.GetDefaultAvatarUrl(),
                Status = userInformation.Status.ToString(),
                CreatedAt = userInformation.CreatedAt,
                JoinedAt = userInformation.JoinedAt,
                FirstSeen = userInformation.FirstSeen,
                LastSeen = userInformation.LastSeen,
                Rank = userRank.Rank,
                Last7DaysMessages = messages7.Sum(x => x.MessageCount),
                Last30DaysMessages = messages30.Sum(x => x.MessageCount),
                AverageMessagesPerDay = userRank.AveragePerDay,
                Percentile = userRank.Percentile,
                Roles = roles
                    .Where(x => !x.IsEveryone)
                    .Select(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Color = x.Color.ToString(),
                    }),
                IsBanned = userInformation.IsBanned,
                BanReason = userInformation.BanReason,
                IsGuildMember = userInformation.GuildId != default
            };

            return Ok(mapped);
        }

        [HttpGet("{userIdString}/messages")]
        public async Task<IActionResult> GetUserMessagesPerChannelAsync(string userIdString, DateTimeOffset after = default)
        {
            if (!ulong.TryParse(userIdString, out var userId))
                return Ok(null);

            var timespan = DateTimeOffset.UtcNow - after;
            var result = await MessageRepository.GetGuildUserMessageCountByChannel(UserGuild.Id, userId, timespan);
            var colors = ColorUtils.GetRainbowColors(result.Count);

            var i = 0;
            var mapped = result.Select(x => new
                               {
                                   Name = x.ChannelName,
                                   Count = x.MessageCount,
                                   Color = colors[i++].ToString()
                               })
                               .OrderByDescending(x => x.Count)
                               .ToList();

            return Ok(mapped);
        }
    }
}
