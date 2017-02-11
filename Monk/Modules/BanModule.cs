using Discord;
using Discord.Commands;
using Monk.Services.Ban;
using System.Threading.Tasks;

namespace Monk.Modules
{
    [Name("Ban"), Summary("Easy way to ban the bad guys.")]
    public class BanModule : ModuleBase
    {
        [Command("ban"), Summary("Bans a bad guy.")]
        public async Task BanAsync(IGuildUser user, int pruneDays = 0, string reason = "")
        {
            await ReplyAsync($"**{user.Mention}** has been **banned** from the server. Reason: {reason}");
            await Context.Guild.AddBanAsync(user, pruneDays);
            await new BanService().BanAsync(user.Id, Context.User.Id, Context.Guild.Id, reason);
        }

        [Command("unban"), Summary("Unbans a guy that has been forgiving.")]
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
