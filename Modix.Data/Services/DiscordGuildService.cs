using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;
using Modix.Data.Utilities;

namespace Modix.Data.Services
{
    public class DiscordGuildService : IDisposable
    {
        private readonly ModixContext _context;

        public DiscordGuildService(ModixContext context)
        {
            _context = context;
        }

        public async Task<DiscordGuild> GetAsync(ulong discordId)
        {

            try
            {
                return await _context.DiscordGuilds
                    .Where(guild => guild.DiscordGuildId == discordId)
                    .Include(guild => guild.Owner)
                    .Include(guild => guild.Config)
                    .FirstAsync();
            }
            catch (InvalidOperationException)
            {
                return null;
            }

        }

        public async Task<DiscordGuild> AddAsync(IGuild guild)
        {

            var service = new DiscordUserService(_context);
            var owner = await guild.GetOwnerAsync();

            var discordGuild = new DiscordGuild()
            {
                Config = new GuildConfig(),
                DiscordGuildId = guild.Id,
                Name = guild.Name,
                CreatedAt = guild.CreatedAt.DateTime,
                Owner = await service.ObtainAsync(owner),
            };

            var res = (await _context.DiscordGuilds.AddAsync(discordGuild)).Entity;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return null;
            }
            return res;

        }

        public async void SetPermissionAsync(IGuild guild, Permissions permission, ulong roleId)
        {
            var discordGuild = await ObtainAsync(guild);

            if (discordGuild.Config == null)
            {
                discordGuild.Config = new GuildConfig
                {
                    GuildId = guild.Id.ToLong(),
                    AdminRoleId = permission == Permissions.Administrator ? roleId.ToLong() : 0,
                    ModeratorRoleId = permission == Permissions.Moderator ? roleId.ToLong() : 0,
                };

                _context.DiscordGuilds.Update(discordGuild);
                await _context.SaveChangesAsync();
                return;
            }

            if (permission == Permissions.Administrator)
            {
                discordGuild.Config.AdminRoleId = roleId.ToLong();
            }
            else
            {
                discordGuild.Config.ModeratorRoleId = roleId.ToLong();
            }

            _context.DiscordGuilds.Update(discordGuild);
            await _context.SaveChangesAsync();
        }

        public async Task<DiscordGuild> ObtainAsync(IGuild guild)
        {
            return await GetAsync(guild.Id) ?? await AddAsync(guild);
        }

        public async Task<bool> AddModuleLimitAsync(DiscordGuild guild, IMessageChannel channel, string module)
        {
            var limit = await _context.ChannelLimits
                .Where(c =>
                    string.Equals(c.ModuleName, module, StringComparison.CurrentCultureIgnoreCase) &&
                    c.Guild.DiscordGuildId == guild.DiscordGuildId &&
                    c.ChannelId == channel.Id.ToLong())
                .FirstOrDefaultAsync();

            if (limit != null) return false;
            
            await _context.ChannelLimits.AddAsync(new ChannelLimit
            {
                ModuleName = module,
                Guild = guild,
                ChannelId = channel.Id.ToLong()
            });

            await _context.SaveChangesAsync();
            return true;


        }

        public async Task<bool> RemoveModuleLimitAsync(DiscordGuild guild, IMessageChannel channel, string module)
        {
            var limit = await _context.ChannelLimits
                .Where(c =>
                    string.Equals(c.ModuleName, module, StringComparison.CurrentCultureIgnoreCase) &&
                    c.Guild.DiscordGuildId == guild.DiscordGuildId &&
                    c.ChannelId == channel.Id.ToLong())
                .FirstOrDefaultAsync();

            if (limit == null)
                return false;
            
            _context.ChannelLimits.Remove(limit);

            await _context.SaveChangesAsync();
            return true;


        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }
    }
}
