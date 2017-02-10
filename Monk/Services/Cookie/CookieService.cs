using Monk.Data.Repositorys;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Monk.Services.Cookie
{
    public class CookieService
    {
        CookieRepository repository = new CookieRepository();

        public async Task<bool> AddCookie(ulong guild, ulong user)
        {
            var cookie = repository.GetOneAsync() // I need to find a way to pass in a condition here :/
        }
    }
}
