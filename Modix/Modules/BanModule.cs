using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Data.Utilities;
using Modix.Services.Ban;
using Modix.Utilities;

namespace Modix.Modules
{
    [Name("Ban"), Summary("Easy way to ban the bad guys.")]
    public class BanModule : ModuleBase
    {
        [Command("ban"), Summary("Bans a bad guy."), RequireModixPermission(Permissions.Administrator | Permissions.Moderator)]
        public async Task BanAsync(IGuildUser user, int pruneDays = 0, string reason = "")
        {
            await ReplyAsync($"**{user.Mention}** has been **banned** from the server. Reason: {reason}");
            await Context.Guild.AddBanAsync(user, pruneDays);
            await new BanService().BanAsync(Context.User as IGuildUser, user, Context.Guild, reason);
        }

        [Command("unban"), Summary("Unbans a guy that has been forgiving."), RequireModixPermission(Permissions.Administrator | Permissions.Moderator)]
        public async Task UnbanAsync(IGuildUser user)
        {
            await Context.Guild.RemoveBanAsync(user.Id);
            await new BanService().UnbanAsync(Context.Guild.Id, user.Id);
            await ReplyAsync($"{user.Mention} has been unbanned.");
        }

        [Command("baninfo"), Summary("Shows ALL bans for a given user in ALL guilds using MODiX")]
        public async Task BanInfo(IGuildUser user)
        {
            var res = await new BanService().GetAllBans(user);
            await ReplyAsync(res);
        }
    }
}
