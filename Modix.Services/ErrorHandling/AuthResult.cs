using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;
using Modix.Common.ErrorHandling;
using Modix.Data.Models.Core;

namespace Modix.Services.ErrorHandling
{
    /// <summary>
    /// A <see cref="ServiceResult"/> representing a failure to execute a request due to the user missing claims
    /// </summary>
    public class AuthResult : ServiceResult
    {
        private readonly IReadOnlyDictionary<AuthorizationClaim, bool> _hasClaimLookup;

        /// <summary>
        /// The claims that were required for the request
        /// </summary>
        public IReadOnlyList<AuthorizationClaim> RequiredClaims { get; private set; }
        /// <summary>
        /// The claims that the current user has
        /// </summary>
        public IReadOnlyList<AuthorizationClaim> CurrentClaims { get; private set; }

        /// <summary>
        /// Creates an <see cref="InsufficientRankResult"/> using the given collections of <see cref="AuthorizationClaim"/>
        /// </summary>
        /// <param name="requiredClaims">The claims that are required for the request</param>
        /// <param name="currentClaims">The claims that the current user has</param>
        public AuthResult(IEnumerable<AuthorizationClaim> requiredClaims, IEnumerable<AuthorizationClaim> currentClaims)
        {
            //Copy these when they come in to avoid issues if they are later modified
            RequiredClaims = new List<AuthorizationClaim>(requiredClaims).AsReadOnly();
            CurrentClaims = new List<AuthorizationClaim>(currentClaims).AsReadOnly();

            //We succeeded if the result of this except operation is an empty collection
            IsSuccess = RequiredClaims.Except(CurrentClaims).Any() == false;

            //Set this up for rendering missing claims later
            _hasClaimLookup = RequiredClaims
                .ToDictionary(d => d, d => CurrentClaims.Contains(d));

            Error = ToString();
        }

        /// <summary>
        /// Returns true if the user had the given claim when the request was executed, false if not
        /// </summary>
        /// <param name="claim">The claim to check for</param>
        public bool HadClaim(AuthorizationClaim claim)
            => _hasClaimLookup.ContainsKey(claim)
                   ? _hasClaimLookup[claim]
                   : false;

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("The following claims are required:");
            builder.AppendLine(RequiredClaims.Humanize());

            builder.AppendLine();

            builder.AppendLine("The following claims are present:");
            builder.AppendLine(CurrentClaims.Humanize());

            return builder.ToString();
        }
    }
}
