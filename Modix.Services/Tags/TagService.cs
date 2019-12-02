using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Utilities;

namespace Modix.Services.Tags
{
    /// <summary>
    /// Describes a service for maintaining and invoking tags.
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild to which the tag will belong.</param>
        /// <param name="creatorId">The Discord snowflake ID of the user who is creating the tag.</param>
        /// <param name="name">The name that will be used to invoke the tag.</param>
        /// <param name="content">The text that will be displayed when the tag is invoked.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <exception cref="ArgumentException">Throws for <paramref name="content"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        Task CreateTagAsync(ulong guildId, ulong creatorId, string name, string content);

        /// <summary>
        /// Invokes a tag.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild to which the tag belongs.</param>
        /// <param name="channelId">The Discord snowflake ID of the channel in which the tag is being invoked.</param>
        /// <param name="name">The name that will be used to invoke the tag.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        Task UseTagAsync(ulong guildId, ulong channelId, string name);

        /// <summary>
        /// Modifies the contents of a tag.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild to which the tag belongs.</param>
        /// <param name="modifierId">The Discord snowflake ID of the user who is modifying the tag.</param>
        /// <param name="name">The name that is used to invoke the tag.</param>
        /// <param name="newContent">The text that will be displayed when the tag is invoked.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <exception cref="ArgumentException">Throws for <paramref name="newContent"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        Task ModifyTagAsync(ulong guildId, ulong modifierId, string name, string newContent);

        /// <summary>
        /// Deletes a tag.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild to which the tag belongs.</param>
        /// <param name="deleterId">The Discord snowflake ID of the user who is modifying the tag.</param>
        /// <param name="name">The name that is used to invoke the tag.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        Task DeleteTagAsync(ulong guildId, ulong deleterId, string name);

        /// <summary>
        /// Searches all tags based on the supplied criteria.
        /// </summary>
        /// <param name="criteria">Criteria describing how to filter the result set of tags.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="criteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes,
        /// with a collection of tags that fit the supplied criteria.
        /// </returns>
        Task<IReadOnlyCollection<TagSummary>> GetSummariesAsync(TagSearchCriteria criteria);

        /// <summary>
        /// Gets all tags owned by the supplied user.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="userId">The Discord snowflake ID of the user who owns the tag.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a collection of all tags owned by the supplied user.
        /// </returns>
        Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Gets all tags owned by the supplied role.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="roleId">The Discord snowflake ID of the role that owns the tag.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a collection of all tags owned by the supplied role.
        /// </returns>
        Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByRoleAsync(ulong guildId, ulong roleId);

        /// <summary>
        /// Transfers a tag to the supplied user.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="name">The name that is used to invoke the tag.</param>
        /// <param name="currentUserId">The user who is attempting to transfer the tag.</param>
        /// <param name="userId">The user who will own the tag.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task TransferToUserAsync(ulong guildId, string name, ulong currentUserId, ulong userId);

        /// <summary>
        /// Transfers a tag to the supplied role.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="name">The name that is used to invoke the tag.</param>
        /// <param name="currentUserId">The user who is attempting to transfer the tag.</param>
        /// <param name="roleId">The role that will own the tag.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task TransferToRoleAsync(ulong guildId, string name, ulong currentUserId, ulong roleId);

        /// <summary>
        /// Determines whether the supplied user can maintain the supplied tag.
        /// </summary>
        /// <param name="tag">The tag to be maintained.</param>
        /// <param name="userId">The Discord snowflake ID of the user who is attempting to maintain the tag.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="tag"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation is complete,
        /// containing a flag indicating whether the user can maintain the tag.
        /// </returns>
        Task<bool> CanUserMaintainTagAsync(TagSummary tag, ulong userId);

        /// <summary>
        /// Determines whether a tag with the given name exists within the given guild.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="name">The name that is used to invoke the tag.</param>
        /// <returns>True if the tag exists, false if not</returns>
        Task<bool> TagExistsAsync(ulong guildId, string name);
    }

    /// <inheritdoc />
    internal class TagService : ITagService
    {
        private static readonly Regex _tagNameRegex = new Regex(@"^\S+\b$");
        
        /// <summary>
        /// Constructs a new <see cref="TagService"/> with the supplied dependencies.
        /// </summary>
        public TagService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            ITagRepository tagRepository,
            IUserService userService,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            TagRepository = tagRepository;
            UserService = userService;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
        }

        /// <inheritdoc />
        public async Task CreateTagAsync(ulong guildId, ulong creatorId, string name, string content)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.CreateTag);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("The tag content cannot be blank or whitespace.", nameof(content));
                
            if (!_tagNameRegex.IsMatch(name))
                throw new ArgumentException("The tag name cannot have punctuation at the end.", nameof(name));

            name = name.Trim().ToLower();

            using (var transaction = await TagRepository.BeginMaintainTransactionAsync())
            {
                var existingTag = await TagRepository.ReadSummaryAsync(guildId, name);

                if (!(existingTag is null))
                    throw new InvalidOperationException($"A tag with the name '{name}' already exists.");

                await TagRepository.CreateAsync(new TagCreationData()
                {
                    GuildId = guildId,
                    CreatedById = creatorId,
                    Name = name,
                    Content = content,
                    Uses = 0,
                });

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task UseTagAsync(ulong guildId, ulong channelId, string name)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.UseTag);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            using (var transaction = await TagRepository.BeginUseTransactionAsync())
            {
                var tag = await TagRepository.ReadSummaryAsync(guildId, name);

                if (tag is null)
                    throw new InvalidOperationException($"The tag '{name}' does not exist.");

                var channel = await DiscordClient.GetChannelAsync(channelId);

                if (!(channel is IMessageChannel messageChannel))
                    throw new InvalidOperationException($"The channel '{channel.Name}' is not a message channel.");

                var sanitizedContent = FormatUtilities.SanitizeAllMentions(tag.Content);

                try
                {
                    await messageChannel.SendMessageAsync(sanitizedContent);
                }
                finally
                {
                    await TagRepository.TryIncrementUsesAsync(guildId, name);
                }

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task ModifyTagAsync(ulong guildId, ulong modifierId, string name, string newContent)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentException("The tag content cannot be blank or whitespace.", nameof(newContent));

            name = name.Trim().ToLower();

            using (var transaction = await TagRepository.BeginMaintainTransactionAsync())
            {
                var tag = await TagRepository.ReadSummaryAsync(guildId, name);

                if (tag is null)
                    throw new InvalidOperationException($"The tag '{name}' does not exist.");

                await EnsureUserCanMaintainTagAsync(tag, modifierId);

                await TagRepository.TryModifyAsync(guildId, name, modifierId, x => x.Content = newContent);

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task DeleteTagAsync(ulong guildId, ulong deleterId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            using (var transaction = await TagRepository.BeginMaintainTransactionAsync())
            {
                var tag = await TagRepository.ReadSummaryAsync(guildId, name);

                if (tag is null)
                    throw new InvalidOperationException($"The tag '{name}' does not exist.");

                await EnsureUserCanMaintainTagAsync(tag, deleterId);

                await TagRepository.TryDeleteAsync(guildId, name, deleterId);

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TagSummary>> GetSummariesAsync(TagSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            return await TagRepository.SearchSummariesAsync(criteria);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByUserAsync(ulong guildId, ulong userId)
            => await TagRepository.SearchSummariesAsync(new TagSearchCriteria()
            {
                GuildId = guildId,
                OwnerUserId = userId,
            });

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByRoleAsync(ulong guildId, ulong roleId)
            => await TagRepository.SearchSummariesAsync(new TagSearchCriteria()
            {
                GuildId = guildId,
                OwnerRoleId = roleId,
            });

        /// <inheritdoc />
        public async Task TransferToUserAsync(ulong guildId, string name, ulong currentUserId, ulong userId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            using (var transaction = await TagRepository.BeginMaintainTransactionAsync())
            {
                var tag = await TagRepository.ReadSummaryAsync(guildId, name);

                if (tag is null)
                    throw new InvalidOperationException($"The tag '{name}' does not exist.");

                await EnsureUserCanMaintainTagAsync(tag, currentUserId);

                await TagRepository.TryModifyAsync(tag.GuildId, tag.Name, currentUserId,
                    x =>
                    {
                        x.OwnerRoleId = null;
                        x.OwnerUserId = userId;
                    });

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task TransferToRoleAsync(ulong guildId, string name, ulong currentUserId, ulong roleId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            using (var transaction = await TagRepository.BeginMaintainTransactionAsync())
            {
                var tag = await TagRepository.ReadSummaryAsync(guildId, name);

                if (tag is null)
                    throw new InvalidOperationException($"The tag '{name}' does not exist.");

                await EnsureUserCanMaintainTagAsync(tag, currentUserId);

                await TagRepository.TryModifyAsync(tag.GuildId, tag.Name, currentUserId,
                    x =>
                    {
                        x.OwnerRoleId = roleId;
                        x.OwnerUserId = null;
                    });

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task<bool> CanUserMaintainTagAsync(TagSummary tag, ulong userId)
        {
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));

            var currentUser = await UserService.GetGuildUserAsync(tag.GuildId, userId);

            if (!await CanTriviallyMaintainTagAsync(currentUser))
            {
                if (tag.OwnerUser is null)
                {
                    if (!await CanUserMaintainTagOwnedByRoleAsync(currentUser, tag.OwnerRole))
                        return false;
                }
                else if (userId != tag.OwnerUser.Id)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// A client for interacting with the Discord API.
        /// </summary>
        protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// A service for interacting with frontend authentication system and performing authorization.
        /// </summary>
        protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// A service for storing and retrieving tag data.
        /// </summary>
        protected ITagRepository TagRepository { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService { get; }

        /// <summary>
        /// An <see cref="IDesignatedRoleMappingRepository"/> storing and retrieving roles designated for use by the application.
        /// </summary>
        internal protected IDesignatedRoleMappingRepository DesignatedRoleMappingRepository { get; }

        private async Task EnsureUserCanMaintainTagAsync(TagSummary tag, ulong currentUserId)
        {
            var currentUser = await UserService.GetGuildUserAsync(tag.GuildId, currentUserId);

            if (!await CanTriviallyMaintainTagAsync(currentUser))
            {
                if (tag.OwnerUser is null)
                {
                    if (!await CanUserMaintainTagOwnedByRoleAsync(currentUser, tag.OwnerRole))
                        throw new InvalidOperationException("User rank insufficient to transfer the tag.");
                }
                else if (currentUserId != tag.OwnerUser.Id)
                {
                    throw new InvalidOperationException("User does not own the tag.");
                }
            }
        }

        private async Task<bool> CanUserMaintainTagOwnedByRoleAsync(IGuildUser currentUser, GuildRoleBrief ownerRole)
        {
            Debug.Assert(!(ownerRole is null));

            var rankRoles = await GetRankRolesAsync(currentUser.GuildId);

            // If the owner role is no longer ranked, everything outranks it.
            if (!rankRoles.Any(x => x.Id == ownerRole.Id))
                return true;

            var currentUserRankRoles = rankRoles.Where(r => currentUser.RoleIds.Contains(r.Id));

            var currentUserMaxRank = currentUserRankRoles.Any()
                ? currentUserRankRoles.Select(x => x.Position).Max()
                : int.MinValue;

            // Only allow maintenance if the user has sufficient rank.
            return currentUserMaxRank >= ownerRole.Position;
        }

        private async Task<bool> CanTriviallyMaintainTagAsync(IGuildUser currentUser)
        {
            // The guild owner can always maintain tags.
            if (currentUser.Guild.OwnerId == currentUser.Id)
                return true;

            // Administrators can always maintain tags.
            if (currentUser.GuildPermissions.Administrator)
                return true;

            // Users with the MaintainOtherUserTag claim can always maintain tags.
            if (await AuthorizationService.HasClaimsAsync(currentUser, AuthorizationClaim.MaintainOtherUserTag))
                return true;

            return false;
        }

        private async Task<IEnumerable<GuildRoleBrief>> GetRankRolesAsync(ulong guildId)
            => (await DesignatedRoleMappingRepository
                .SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = guildId,
                    Type = DesignatedRoleType.Rank,
                    IsDeleted = false,
                }))
                .Select(r => r.Role);

        public async Task<bool> TagExistsAsync(ulong guildId, string name)
        {
            var tag = await TagRepository.ReadSummaryAsync(guildId, name);
            return tag != null;
        }
    }
}
