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
    public class DiscordUserService : IDisposable
    {
        private ModixContext _context;

        public DiscordUserService(ModixContext context)
        {
            _context = context;
        }
        public async Task<DiscordUser> GetAsync(IGuildUser user)
        {
            try
            {
                return await _context.Users.SingleAsync(x => x.DiscordId == user.Id.ToLong());
            }
            catch (InvalidOperationException)
            {
                return null;
            }

        }

        public async Task<DiscordUser> AddAsync(IGuildUser user)
        {
            var discordUser = new DiscordUser()
            {
                DiscordId = user.Id.ToLong(),
                AvatarUrl = user.GetAvatarUrl(),
                CreatedAt = user.CreatedAt.DateTime,
                IsBot = user.IsBot,
                Username = user.Username,
                Nickname = user.Nickname,
            };

            var res = (await _context.Users.AddAsync(discordUser)).Entity;
            await _context.SaveChangesAsync();
            return res;

        }

        public async Task<DiscordUser> ObtainAsync(IGuildUser user)
        {
            return await GetAsync(user) ?? await AddAsync(user);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }
    }
}
