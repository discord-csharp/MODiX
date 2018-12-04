using System;
using System.Collections.Generic;
using System.Text;
using Modix.Common.Test;
using Modix.Data.Models.Core;
using Modix.Services.ErrorHandling;
using NUnit.Framework;
using Shouldly;

namespace Modix.Services.Test
{
    public class AuthResultTests
    {
        [Test]
        public void AuthResult_IsSuccess_IfUserHasClaims()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationWarn
            };

            var hasClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationRead,
                AuthorizationClaim.ModerationWarn
            };

            var authResult = new AuthResult(requiredClaims, hasClaims);

            authResult.ShouldBeSuccessful();
        }

        [Test]
        public void AuthResult_IsFailure_IfUserMissingClaims()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationWarn
            };

            var hasClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationRead
            };

            var authResult = new AuthResult(requiredClaims, hasClaims);

            authResult.ShouldBeFailure();
        }

        [Test]
        public void AuthResult_IsFailure_IfUserHasNoClaims()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationWarn
            };

            var hasClaims = new AuthorizationClaim[] { };

            var authResult = new AuthResult(requiredClaims, hasClaims);

            authResult.ShouldBeFailure();
        }

        [Test]
        public void AuthResult_IsSuccess_IfNoClaimsRequired_AndUserHasNoClaims()
        {
            var requiredClaims = new AuthorizationClaim[] { };
            var hasClaims = new AuthorizationClaim[] { };

            var authResult = new AuthResult(requiredClaims, hasClaims);

            authResult.ShouldBeSuccessful();
        }
    }
}
