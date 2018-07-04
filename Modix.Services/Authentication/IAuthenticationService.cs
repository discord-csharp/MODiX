using System;
using System.Collections.Generic;
using System.Text;

using Modix.Services.Authorization;

namespace Modix.Services.Authentication
{
    public interface IAuthenticationService
    {
        long? CurrentUserId { get; }

        ICollection<AuthorizationClaims> Claims { get; }
    }
}
