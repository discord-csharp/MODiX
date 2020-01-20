using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Repositories;
using Modix.Services.Core;

namespace Modix.Controllers
{
    [Route("~/api/userInformation")]
    public class UserInformationController : ModixController
    {
        private readonly IUserService _userService;

        private readonly IMessageRepository _messageRepository;

        public UserInformationController(
            DiscordSocketClient client,
            IAuthorizationService modixAuth,
            IUserService userService,
            IMessageRepository messageRepository)
            : base(client, modixAuth)
        {
            _userService = userService;
            _messageRepository = messageRepository;
        }

        [HttpGet("{userIdString}")]
        public async Task<IActionResult> GetUserInformationAsync(string userIdString)
        {
            if (!ulong.TryParse(userIdString, out var userId))
                return Ok(null);

            var userInformation = await _userService.GetUserInformationAsync(UserGuild.Id, userId);

            if (userInformation is null)
                return Ok(null);

            var userRank = await _messageRepository.GetGuildUserParticipationStatistics(UserGuild.Id, userId);
            var messages7 = await _messageRepository.GetGuildUserMessageCountByDate(UserGuild.Id, userId, TimeSpan.FromDays(7));
            var messages30 = await _messageRepository.GetGuildUserMessageCountByDate(UserGuild.Id, userId, TimeSpan.FromDays(30));

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
                Last7DaysMessages = messages7.Sum(x => x.Value),
                Last30DaysMessages = messages30.Sum(x => x.Value),
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
    }
}
