using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Modix
{
    public class LogFilesAuthorizationMiddleware
    {
        public LogFilesAuthorizationMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(
            HttpContext context)
        {
            var userIdClaim = context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
                return context.ChallengeAsync();

            var userId = ulong.Parse(userIdClaim.Value);
            if (!_maintainerUserIds.Contains(userId))
                return context.ForbidAsync();

            return _next.Invoke(context);
        }

        private static readonly IReadOnlyCollection<ulong> _maintainerUserIds
            = new HashSet<ulong>()
            {
                135910101667020800, // Cisien#9278
                297508715408654336, // distilld#1111
                137791696325836800, // JakenVeina#1758
                213437573618597888, // Scott#9000
                104975006542372864, // patrickk#0001
                186887742108598272, // calledude
            };

        private readonly RequestDelegate _next;
    }
}
