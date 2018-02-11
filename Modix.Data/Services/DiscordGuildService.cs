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
        private ModixContext _context;

        public DiscordGuildService(ModixContext context)
        {
            _context = context;
        }

        public async Task<DiscordGuild> GetAsync(ulong discordId)
        {

            try
            {
                return await _context.Guilds
                    .Where(guild => guild.DiscordId == discordId.ToLong())
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

            var service = new DiscordUserService(new ModixContext());
            var owner = await guild.GetOwnerAsync();

            var discordGuild = new DiscordGuild()
            {
                Config = new GuildConfig(),
                DiscordId = guild.Id.ToLong(),
                Name = guild.Name,
                CreatedAt = guild.CreatedAt.DateTime,
                Owner = await service.ObtainAsync(owner),
            };

            var res = (await _context.Guilds.AddAsync(discordGuild)).Entity;
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
                discordGuild.Config = new Data.Models.GuildConfig
                {
                    GuildId = guild.Id.ToLong(),
                    AdminRoleId = permission == Permissions.Administrator ? roleId.ToLong() : 0,
                    ModeratorRoleId = permission == Permissions.Moderator ? roleId.ToLong() : 0,
                };

                _context.Guilds.Update(discordGuild);
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

            _context.Guilds.Update(discordGuild);
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
                    c.ModuleName.ToUpper() == module.ToUpper() &&
                    c.Guild.Id == guild.Id &&
                    c.ChannelId == channel.Id.ToLong())
                .FirstOrDefaultAsync();

            if (limit == null)
            {
                await _context.ChannelLimits.AddAsync(new ChannelLimit
                {
                    ModuleName = module,
                    Guild = guild,
                    ChannelId = channel.Id.ToLong()
                });

                await _context.SaveChangesAsync();
                return true;
            }


            return false;
        }

        public async Task<bool> RemoveModuleLimitAsync(DiscordGuild guild, IMessageChannel channel, string module)
        {
            var limit = await _context.ChannelLimits
                .Where(c =>
                    c.ModuleName.ToUpper() == module.ToUpper() &&
                    c.Guild.Id == guild.Id &&
                    c.ChannelId == channel.Id.ToLong())
                .FirstOrDefaultAsync();

            if (limit != null)
            {
                _context.ChannelLimits.Remove(limit);

                await _context.SaveChangesAsync();
                return true;
            }


            return false;
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }
    }
}
