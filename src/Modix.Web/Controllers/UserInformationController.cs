using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Utilities;
using Modix.Web.Shared.Models.Common;
using Modix.Web.Shared.Models.UserLookup;

namespace Modix.Web.Controllers;

[Route("~/api/userinformation")]
[ApiController]
[Authorize]
public class UserInformationController : ModixController
{
    private readonly IUserService _userService;
    private readonly IMessageRepository _messageRepository;

    public UserInformationController(IUserService userService, IMessageRepository messageRepository, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _userService = userService;
        _messageRepository = messageRepository;
    }


    [HttpGet("{userIdString}")]
    public async Task<UserInformation?> GetUserInformationAsync(string userIdString)
    {
        if (!ulong.TryParse(userIdString, out var userId))
            return null;

        var userInformation = await _userService.GetUserInformationAsync(UserGuild.Id, userId);
        if (userInformation is null)
            return null;

        var userRank = await _messageRepository.GetGuildUserParticipationStatistics(UserGuild.Id, userId);
        var messages7 = await _messageRepository.GetGuildUserMessageCountByDate(UserGuild.Id, userId, TimeSpan.FromDays(7));
        var messages30 = await _messageRepository.GetGuildUserMessageCountByDate(UserGuild.Id, userId, TimeSpan.FromDays(30));

        var roles = userInformation.RoleIds
            .Select(x => UserGuild.GetRole(x))
            .OrderByDescending(x => x.IsHoisted)
            .ThenByDescending(x => x.Position)
            .ToArray();

        return FromEphemeralUser(userInformation, userRank, messages7, messages30, roles);
    }

    [HttpGet("{userIdString}/messages")]
    public async Task<MessageCountPerChannelInformation[]> GetUserMessagesPerChannelAsync(string userIdString, DateTimeOffset after = default)
    {
        if (!ulong.TryParse(userIdString, out var userId))
            return [];

        var timespan = DateTimeOffset.UtcNow - after;
        var result = await _messageRepository.GetGuildUserMessageCountByChannel(UserGuild.Id, userId, timespan);
        var colors = ColorUtils.GetRainbowColors(result.Count);

        return result.Select((x,i) => new MessageCountPerChannelInformation(x.ChannelName, x.MessageCount, colors[i].ToString()))
                     .OrderByDescending(x => x.Count)
                     .ToArray();
    }

    private static UserInformation FromEphemeralUser(
        EphemeralUser ephemeralUser,
        GuildUserParticipationStatistics userRank,
        IReadOnlyList<MessageCountByDate> messages7,
        IReadOnlyList<MessageCountByDate> messages30,
        SocketRole[] roles)
    {
        return new UserInformation(
            ephemeralUser.Id.ToString(),
            ephemeralUser.Username,
            ephemeralUser.Nickname,
            ephemeralUser.Discriminator,
            ephemeralUser.AvatarId != null ? ephemeralUser.GetAvatarUrl(ImageFormat.Auto, 256) : ephemeralUser.GetDefaultAvatarUrl(),
            ephemeralUser.CreatedAt,
            ephemeralUser.JoinedAt,
            ephemeralUser.FirstSeen,
            ephemeralUser.LastSeen,
            userRank.Rank,
            messages7.Sum(x => x.MessageCount),
            messages30.Sum(x => x.MessageCount),
            userRank.AveragePerDay,
            userRank.Percentile,
            roles
                .Where(x => !x.IsEveryone)
                .Select(x => new RoleInformation(x.Id, x.Name, x.Color.ToString())),
            ephemeralUser.IsBanned,
            ephemeralUser.BanReason,
            ephemeralUser.GuildId != default
        );
    }
}
