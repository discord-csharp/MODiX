using Discord;
using Discord.WebSocket;
using Modix.Services.Core;
using Modix.Web.Models;
using Modix.Web.Shared.Models.Common;

namespace Modix.Web.Services;

public class DiscordHelper(DiscordSocketClient client, IUserService userService, SessionState sessionState)
{
    public SocketGuild GetUserGuild()
    {
        if (sessionState.SelectedGuild != 0)
            return client.GetGuild(sessionState.SelectedGuild);

        return client.Guilds.First();
    }

    public IEnumerable<ChannelInformation> AutocompleteChannels(string query)
    {
        if (query.StartsWith('#'))
        {
            query = query[1..];
        }

        var currentGuild = GetUserGuild();
        return currentGuild.Channels
            .Where(d => d is SocketTextChannel
                && d.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .Select(d => new ChannelInformation(d.Id, d.Name));
    }
}
