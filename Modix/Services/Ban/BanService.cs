using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Services;
using Modix.Data.Utilities;
using Modix.Utilities;

namespace Modix.Services.Ban
{
    public class BanService
    {
        private readonly DiscordGuildService _guildService = new DiscordGuildService(new ModixContext());

        public async Task BanAsync(IGuildUser author, IGuildUser user, IGuild guild, string reason)
        {
            using (var db = new ModixContext())
            {
                var dbGuild = await _guildService.ObtainAsync(guild);

                var ban = new Data.Models.Ban
                {
                    CreatorId = author.Id.ToLong(),
                    UserId = user.Id.ToLong(),
                    Reason = reason,
                    Guild = dbGuild,
                };

                await db.Bans.AddAsync(ban);
            }
        }

        public async Task UnbanAsync(ulong guildId, ulong userId)
        {
            using (var db = new ModixContext())
            {
                var banResult = await db.Bans.AsQueryable()
                                    .Where(ban => ban.UserId == userId.ToLong() && ban.Guild.Id == guildId.ToLong())
                                    .FirstAsync();

                banResult.Active = false;
                db.Bans.Update(banResult);
            }
        }

        public async Task<string> GetAllBans(IGuildUser user)
        {
            using (var db = new ModixContext())
            {
                var bans = db.Bans.Where(x => x.Active && x.Guild.Id == user.GuildId.ToLong()).ToAsyncEnumerable();

                var sb = new StringBuilder();
                await bans.ForEachAsync(ban => sb.AppendLine($"{ban.Reason} | Active: {ban.Active}"));
                return sb.ToString();
            }
        }
    }
}
