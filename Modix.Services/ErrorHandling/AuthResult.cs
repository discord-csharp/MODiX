using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Creates an <see cref="AuthResult"/> using the given collections of <see cref="AuthorizationClaim"/>
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
        /// Returns true if the user had the given required claim when the request was executed, false if not
        /// </summary>
        /// <param name="claim">The claim to check for - must have been required</param>
        /// <remarks>Will return false if the given claim is not within the RequiredClaims</remarks>
        public bool HadRequiredClaim(AuthorizationClaim claim)
        {
            var hasKey = _hasClaimLookup.TryGetValue(claim, out var hasClaim);
            return hasKey ? hasClaim : false;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("Required: ");
            builder.AppendLine(string.Join(",", RequiredClaims));

            builder.Append("Present: ");
            builder.AppendLine(string.Join(",", CurrentClaims));

            return builder.ToString();
        }
    }
}
