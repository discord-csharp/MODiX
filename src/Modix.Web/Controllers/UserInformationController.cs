using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Utilities;
using Modix.Web.Models;
using Modix.Web.Shared.Models.Common;
using Modix.Web.Shared.Models.UserLookup;

namespace Modix.Web.Controllers;

[Route("~/api/userinformation")]
[ApiController]
[Authorize]
public class UserInformationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMessageRepository _messageRepository;
    private readonly DiscordSocketClient _discordSocketClient;

    public UserInformationController(IUserService userService, IMessageRepository messageRepository, DiscordSocketClient discordSocketClient)
    {
        _userService = userService;
        _messageRepository = messageRepository;
        _discordSocketClient = discordSocketClient;
    }


    [HttpGet("{userIdString}")]
    public async Task<UserInformation?> GetUserInformationAsync(string userIdString)
    {
        if (!ulong.TryParse(userIdString, out var userId))
            return null;

        // TODO: Move this to a base class like ModixController?

        var guildCookie = Request.Cookies[CookieConstants.SelectedGuild];

        SocketGuild guildToSearch;
        if (!string.IsNullOrWhiteSpace(guildCookie))
        {
            var guildId = ulong.Parse(guildCookie);
            guildToSearch = _discordSocketClient.GetGuild(guildId);
        }
        else
        {
            guildToSearch = _discordSocketClient.Guilds.First();
        }

        var userInformation = await _userService.GetUserInformationAsync(guildToSearch.Id, userId);
        if (userInformation is null)
            return null;

        var userRank = await _messageRepository.GetGuildUserParticipationStatistics(guildToSearch.Id, userId);
        var messages7 = await _messageRepository.GetGuildUserMessageCountByDate(guildToSearch.Id, userId, TimeSpan.FromDays(7));
        var messages30 = await _messageRepository.GetGuildUserMessageCountByDate(guildToSearch.Id, userId, TimeSpan.FromDays(30));

        var roles = userInformation.RoleIds
            .Select(x => guildToSearch.GetRole(x))
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

        var guildCookie = Request.Cookies[CookieConstants.SelectedGuild];

        SocketGuild guildToSearch;
        if (!string.IsNullOrWhiteSpace(guildCookie))
        {
            var guildId = ulong.Parse(guildCookie);
            guildToSearch = _discordSocketClient.GetGuild(guildId);
        }
        else
        {
            guildToSearch = _discordSocketClient.Guilds.First();
        }

        var timespan = DateTimeOffset.UtcNow - after;
        var result = await _messageRepository.GetGuildUserMessageCountByChannel(guildToSearch.Id, userId, timespan);
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
