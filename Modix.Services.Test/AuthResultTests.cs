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
        public void Constructor_OwnedClaimsContainsRequiredClaims_IsSuccess()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationWarn
            };

            var ownedClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationRead,
                AuthorizationClaim.ModerationWarn
            };

            var authResult = new AuthResult(requiredClaims, ownedClaims);

            authResult.ShouldBeSuccessful();
        }

        [Test]
        public void Constructor_OwnedClaimsMissingRequiredClaims_IsFailure()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationWarn
            };

            var ownedClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationRead
            };

            var authResult = new AuthResult(requiredClaims, ownedClaims);

            authResult.ShouldBeFailure();
        }

        [Test]
        public void Constructor_OwnedClaimsEmpty_IsFailure()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationWarn
            };

            var ownedClaims = new AuthorizationClaim[] { };

            var authResult = new AuthResult(requiredClaims, ownedClaims);

            authResult.ShouldBeFailure();
        }

        [Test]
        public void Constructor_OwnedAndRequiredClaimsEmpty_IsSuccess()
        {
            var requiredClaims = new AuthorizationClaim[] { };
            var ownedClaims = new AuthorizationClaim[] { };

            var authResult = new AuthResult(requiredClaims, ownedClaims);

            authResult.ShouldBeSuccessful();
        }

        [Test]
        public void HadRequiredClaim_OwnedClaimsContainsARequiredClaim_IsTrue()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationRead,
                AuthorizationClaim.ModerationWarn
            };

            var ownedClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationRead,
                AuthorizationClaim.ModerationWarn
            };

            var authResult = new AuthResult(requiredClaims, ownedClaims);

            authResult.HadRequiredClaim(AuthorizationClaim.ModerationRead).ShouldBeTrue();
        }

        [Test]
        public void HadRequiredClaim_OwnedClaimsMissingARequiredClaim_IsFalse()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationRead,
                AuthorizationClaim.ModerationWarn
            };

            var ownedClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationWarn
            };

            var authResult = new AuthResult(requiredClaims, ownedClaims);

            authResult.HadRequiredClaim(AuthorizationClaim.ModerationRead).ShouldBeFalse();
        }

        [Test]
        public void HadRequiredClaim_CheckedClaimWasNotRequired_IsFalse()
        {
            var requiredClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationRead,
                AuthorizationClaim.ModerationWarn
            };

            var ownedClaims = new AuthorizationClaim[]
            {
                AuthorizationClaim.ModerationWarn
            };

            var authResult = new AuthResult(requiredClaims, ownedClaims);

            authResult.HadRequiredClaim(AuthorizationClaim.PostInviteLink).ShouldBeFalse();
        }
    }
}
