using Discord.Commands;
using Monk.Data.Repositorys;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Monk.Services.Cookies;

namespace Monk.Modules
{
    public class CookieModule : ModuleBase
    {
        /// <summary>
        /// In-memory caching of who gave whom a cookie to avoid cookie spam.
        /// <remark>
        ///     Static because each Module is started in its own instance.
        /// </remark>
        /// </summary>
        private static Dictionary<ulong, DateTime> cookieLog = new Dictionary<ulong, DateTime>();

        [Command("add"), Alias("thanks")]
        public async Task AddCookieAsync(IGuildUser user)
        {
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("You can´t thank yourself!");
                return;
            }

            if (cookieLog.ContainsKey(user.Id) && (cookieLog[user.Id].Subtract(DateTime.Now).Minutes < 60))
            {
                await ReplyAsync("You are thanking too much!");
                return;
            }

            var cookieService = new CookieService();
            await cookieService.AddCookie(user.GuildId, user.Id);
            await ReplyAsync($"A cookie has been added to {user.Username}. Its ugly, but i just want to test if my DAL works.");

            cookieLog.Add(user.Id, DateTime.Now);
        }

        [Command("count")]
        public async Task GetCookieCountAsync(IGuildUser user)
        {
            var cookies = await new CookieService().GetCookieCount(user.GuildId, user.Id);
            await ReplyAsync($"{user.Username} has {cookies} cookies!");
        }
    }
}
