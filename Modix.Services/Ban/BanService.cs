using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Services;

namespace Modix.Services.Ban
{
    public class BanService
    {
        private readonly ModixContext _context;
        private readonly DiscordGuildService _guildService;

        public BanService(ModixContext context)
        {
            _context = context;
            _guildService = new DiscordGuildService(context);
        }

        public async Task BanAsync(IGuildUser author, IGuildUser user, IGuild guild, string reason)
        {
            var dbGuild = await _guildService.ObtainAsync(guild);

            var ban = new Data.Models.Ban
            {
                DiscordCreatorID = author.Id,
                DiscordUserID = user.Id,
                Reason = reason,
                Guild = dbGuild
            };

            await _context.Bans.AddAsync(ban);
            await _context.SaveChangesAsync();
        }

        public async Task UnbanAsync(ulong guildId, ulong userId)
        {
            var banResult = await _context.Bans.AsQueryable()
                .FirstAsync(ban => ban.DiscordUserID == userId && ban.Guild.DiscordGuildID == guildId);

            banResult.Active = false;
            _context.Bans.Update(banResult);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetAllBans(IGuildUser user)
        {
            var bans = _context.Bans.Where(x => x.Active && x.Guild.DiscordGuildID == user.GuildId).ToAsyncEnumerable();

            var sb = new StringBuilder();
            await bans.ForEachAsync(ban => sb.AppendLine($"{ban.Reason} | Active: {ban.Active}"));
            return sb.ToString();
        }
    }
}