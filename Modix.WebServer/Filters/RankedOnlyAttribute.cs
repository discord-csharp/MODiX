using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Modix.WebServer.Controllers;

namespace Modix.WebServer.Filters
{
    public class RankedOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is ModixController controller &&
                controller.SocketUser.Roles.Count <= 1)
            {
                context.Result = new ContentResult
                {
                    Content = "You need to be ranked to do that.",
                    ContentType = "text/plain",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }
    }
}
