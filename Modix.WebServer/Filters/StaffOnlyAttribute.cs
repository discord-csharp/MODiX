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
    public class StaffOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is ModixController controller && !controller.IsStaff)
            {
                context.Result = new ContentResult
                {
                    Content = "You need to be a staff member to do that.",
                    ContentType = "text/plain",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }
    }
}
