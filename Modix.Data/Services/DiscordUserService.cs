using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;
using Modix.Data.Utilities;

namespace Modix.Data.Services
{
    public class DiscordUserService
    {
        private ModixContext _context;

        public DiscordUserService(ModixContext context)
        {
            _context = context;
        }
        public async Task<DiscordUser> GetAsync(IGuildUser user)
        {
            using (var db = new ModixContext())
            {
                try
                {
                    return await db.Users.SingleAsync(x => x.DiscordId == user.Id.ToLong());
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        public async Task<DiscordUser> AddAsync(IGuildUser user)
        {
            using (var db = new ModixContext())
            {
                var discordUser = new DiscordUser()
                {
                    DiscordId = user.Id.ToLong(),
                    AvatarUrl = user.AvatarUrl,
                    CreatedAt = user.CreatedAt.DateTime,
                    IsBot = user.IsBot,
                    Username = user.Username,
                    Nickname = user.Nickname,
                };

                var res = (await db.Users.AddAsync(discordUser)).Entity;
                await db.SaveChangesAsync();
                return res;
            }
        }

        public async Task<DiscordUser> ObtainAsync(IGuildUser user)
        {
            return await GetAsync(user) ?? await AddAsync(user);
        }
    }
}
