using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Build.Utilities;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;
using Modix.Data.Utilities;

namespace Modix.Data.Services
{
    public class DiscordGuildService
    {
        private ModixContext _context;

        public DiscordGuildService(ModixContext context)
        {
            _context = context;
        }

        public async Task<DiscordGuild> GetAsync(ulong discordId)
        {
            using (var db = new ModixContext())
            {
                try
                {
                    return await db.Guilds
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
        }

        public async Task<DiscordGuild> AddAsync(IGuild guild)
        {
            using (var db = new ModixContext())
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

                var res = (await db.Guilds.AddAsync(discordGuild)).Entity;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return null;
                }
                return res;
            }
        }

        public async void SetPermissionAsync(IGuild guild, Permissions permission, ulong roleId)
        {
            var discordGuild = await ObtainAsync(guild);

            using (var db = new ModixContext())
            {
                if (discordGuild.Config == null)
                {
                    discordGuild.Config = new Data.Models.GuildConfig
                    {
                        GuildId = guild.Id.ToLong(),
                        AdminRoleId = permission == Permissions.Administrator ? roleId.ToLong() : 0,
                        ModeratorRoleId = permission == Permissions.Moderator ? roleId.ToLong() : 0,
                    };

                    db.Guilds.Update(discordGuild);
                    await db.SaveChangesAsync();
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

                db.Guilds.Update(discordGuild);
                await db.SaveChangesAsync();
            }
        }

        public async Task<DiscordGuild> ObtainAsync(IGuild  guild)
        {
            return await GetAsync(guild.Id) ?? await AddAsync(guild);
        }
    }
}
