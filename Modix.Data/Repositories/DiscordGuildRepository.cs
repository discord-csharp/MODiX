using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Build.Utilities;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;
using Modix.Utilities;

namespace Modix.Data.Repositories
{
    public class DiscordGuildRepository
    {
        public async Task<DiscordGuild> GetByGuildAsync(IGuild guild)
        {
            using (var db = new ModixContext())
            {
                try
                {
                    return await db.Guilds.SingleAsync(x => x.DiscordId == guild.Id.ToLong());
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        public async Task<DiscordGuild> AddByGuildAsync(IGuild guild)
        {
            using (var db = new ModixContext())
            {
                var repository = new DiscordUserRepository();
                var owner = await guild.GetOwnerAsync();
                var discordGuild = new DiscordGuild()
                {
                    Config = new GuildConfig(),
                    DiscordId = guild.Id.ToLong(),
                    Name =  guild.Name,
                    CreatedAt = guild.CreatedAt.DateTime,
                    //Todo: Replace with actual database check
                    Owner = await repository.AddByUserAsync(owner) ?? await repository.GetByUserAsync(owner),
                };

                var res =  (await db.Guilds.AddAsync(discordGuild)).Entity;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return res;
                }
                return res;
            }
        }
    }
}
