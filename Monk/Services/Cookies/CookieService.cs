using Monk.Data.Repositorys;
using Monk.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Monk.Services.Cookies
{
    public class CookieService
    {
        CookieRepository repository = new CookieRepository();

        public async Task<bool> AddCookie(ulong guild, ulong user)
        {
            var cookie = repository.GetOne(x => x.GuildId == guild && x.OwnerId == user);

            if (cookie == null)
            {
                cookie = new Cookie
                {
                    GuildId = guild,
                    OwnerId = user,
                    Count = 1,
                };

                await repository.InsertAsync(cookie);
                return true;
            }

            cookie.Count++;
            var res = await repository.Update(cookie.Id, cookie);
            return res.ModifiedCount == 1;
        }

        public async Task<int> GetCookieCount(ulong guild, ulong user)
        {
            return await Task.Run(() => repository.GetOne(x => x.GuildId == guild && x.OwnerId == user).Count);
        }
    }
}
