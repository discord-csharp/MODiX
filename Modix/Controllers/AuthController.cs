using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Modix.Controllers
{
    [Route("~/api")]
    public class AuthController : Controller
    {
        [HttpGet("unauthorized")]
        public IActionResult NeedLogin()
        {
            return Unauthorized();
        }

        [HttpGet("login")]
        public IActionResult LogIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, DiscordAuthenticationDefaults.AuthenticationScheme);
        }
        
        [HttpGet("logout")]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //Make sure we remove the SelectedGuild cookie too
            Response.Cookies.Append("SelectedGuild", "", new CookieOptions
            {
                Expires = DateTimeOffset.MinValue
            });

            return Redirect("/");
        }

        [HttpGet("userInfo")]
        public IActionResult UserInfo()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoggedInUserInfo", "Api");
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
