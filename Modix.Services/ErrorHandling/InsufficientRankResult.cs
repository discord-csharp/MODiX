using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Modix.Common.ErrorHandling;
using Modix.Data.Models.Core;

namespace Modix.Services.ErrorHandling
{
    /// <summary>
    /// A <see cref="ServiceResult"/> representing a failure to execute a request due to insufficient rank
    /// </summary>
    public class InsufficientRankResult : ServiceResult
    {
        /// <summary>
        /// The Id of the required role
        /// </summary>
        public ulong RequiredRoleId { get; private set; }
        /// <summary>
        /// The name of the required role
        /// </summary>
        public string RequiredRoleName { get; private set; }

        /// <summary>
        /// The Id of the current role
        /// </summary>
        public ulong CurrentRoleId { get; private set; }
        /// <summary>
        /// The name of the current role
        /// </summary>
        public string CurrentRoleName { get; private set; }

        /// <summary>
        /// Creates an <see cref="InsufficientRankResult"/> using the given instances of <see cref="IRole"/>
        /// </summary>
        /// <param name="requiredRole">The role that was required by the request</param>
        /// <param name="currentRole">The role that the user had</param>
        public InsufficientRankResult(IRole requiredRole, IRole currentRole)
        {
            //TODO: Move actual rank checking logic into here

            RequiredRoleId = requiredRole.Id;
            RequiredRoleName = requiredRole.Name;

            CurrentRoleId = currentRole.Id;
            CurrentRoleName = currentRole.Name;

            IsSuccess = false;

            Error = ToString();
        }

        /// <summary>
        /// Creates an <see cref="InsufficientRankResult"/> using the given instances of <see cref="GuildRoleBrief"/>
        /// </summary>
        /// <param name="requiredRole">The role that was required by the request</param>
        /// <param name="currentRole">The role that the user had</param>
        public InsufficientRankResult(GuildRoleBrief requiredRole, GuildRoleBrief currentRole)
        {
            if (requiredRole == null || currentRole == null)
            {
                IsSuccess = false;
                Error = "Either you or the target user are missing roles that are configured as \"Rank\"s. Please check your Modix config.";
                return;
            }

            RequiredRoleId = requiredRole.Id;
            RequiredRoleName = requiredRole.Name;

            CurrentRoleId = currentRole.Id;
            CurrentRoleName = currentRole.Name;

            IsSuccess = false;

            Error = ToString();
        }

        public override string ToString()
        {
            return $"Cannot moderate users that have a rank greater than or equal to your own. Your rank: {CurrentRoleName}, required: {RequiredRoleName}";
        }
    }
}
