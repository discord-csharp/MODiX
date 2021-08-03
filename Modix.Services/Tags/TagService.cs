using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Utilities;

namespace Modix.Services.Tags
{
    public interface ITagService
    {
        Task CreateTagAsync(ulong guildId, ulong creatorId, string name, string content);

        Task UseTagAsync(ulong guildId, ulong channelId, string name);

        Task ModifyTagAsync(ulong guildId, ulong modifierId, string name, string newContent);

        Task DeleteTagAsync(ulong guildId, ulong deleterId, string name);

        Task<TagSummary> GetTagAsync(ulong guildId, string name);

        Task<IReadOnlyCollection<TagSummary>> GetSummariesAsync(TagSearchCriteria criteria);

        Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByUserAsync(ulong guildId, ulong userId);

        Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByRoleAsync(ulong guildId, ulong roleId);

        Task TransferToUserAsync(ulong guildId, string name, ulong currentUserId, ulong userId);

        Task TransferToRoleAsync(ulong guildId, string name, ulong currentUserId, ulong roleId);

        Task<bool> TagExistsAsync(ulong guildId, string name);
    }

    internal class TagService : ITagService
    {
        private readonly IDiscordClient _discordClient;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserService _userService;
        private readonly IDesignatedRoleMappingRepository _designatedRoleMappingRepository;
        private readonly ModixContext _modixContext;

        private static readonly Regex _tagNameRegex = new Regex(@"^\S+\b$");

        public TagService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IUserService userService,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository,
            ModixContext modixContext)
        {
            _discordClient = discordClient;
            _authorizationService = authorizationService;
            _userService = userService;
            _modixContext = modixContext;
            _designatedRoleMappingRepository = designatedRoleMappingRepository;
        }

        public async Task CreateTagAsync(ulong guildId, ulong creatorId, string name, string content)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.CreateTag);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("The tag content cannot be blank or whitespace.", nameof(content));

            if (!_tagNameRegex.IsMatch(name))
                throw new ArgumentException("The tag name cannot have punctuation at the end.", nameof(name));

            name = name.Trim().ToLower();

            if (await _modixContext.Set<TagEntity>().Where(x => x.GuildId == guildId).Where(x => x.DeleteActionId == null).AnyAsync(x => x.Name == name))
                throw new InvalidOperationException($"A tag with the name '{name}' already exists.");

            var tag = new TagEntity
            {
                GuildId = guildId,
                OwnerUserId = creatorId,
                Name = name,
                Content = content,
            };

            var createAction = new TagActionEntity()
            {
                GuildId = guildId,
                Created = DateTimeOffset.Now,
                Type = TagActionType.TagCreated,
                CreatedById = creatorId,
            };

            tag.CreateAction = createAction;

            _modixContext.Set<TagEntity>().Add(tag);

            await _modixContext.SaveChangesAsync();
        }

        public async Task UseTagAsync(ulong guildId, ulong channelId, string name)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.UseTag);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            var channel = await _discordClient.GetChannelAsync(channelId);

            if (!(channel is IMessageChannel messageChannel))
                throw new InvalidOperationException($"The channel '{channel.Name}' is not a message channel.");

            var tag = await _modixContext
                .Set<TagEntity>()
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.Name == name)
                .SingleOrDefaultAsync();

            if (tag is null)
                return;

            var sanitizedContent = FormatUtilities.SanitizeAllMentions(tag.Content);

            await messageChannel.SendMessageAsync(sanitizedContent);

            tag.IncrementUse();

            await _modixContext.SaveChangesAsync();
        }

        public async Task ModifyTagAsync(ulong guildId, ulong modifierId, string name, string newContent)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentException("The tag content cannot be blank or whitespace.", nameof(newContent));

            name = name.Trim().ToLower();

            var tag = await _modixContext
                .Set<TagEntity>()
                .Include(x => x.OwnerRole)
                .Include(x => x.OwnerUser)
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.Name == name)
                .SingleOrDefaultAsync();

            if (tag is null)
                return;

            await EnsureUserCanMaintainTagAsync(tag, modifierId);

            tag.Update(newContent);

            await _modixContext.SaveChangesAsync();
        }

        public async Task DeleteTagAsync(ulong guildId, ulong deleterId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            var tag = await _modixContext
                .Set<TagEntity>()
                .Include(x => x.OwnerRole)
                .Include(x => x.OwnerUser)
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.Name == name)
                .SingleOrDefaultAsync();

            if (tag is null)
                return;

            await EnsureUserCanMaintainTagAsync(tag, deleterId);

            tag.Delete(deleterId);

            await _modixContext.SaveChangesAsync();
        }

        public async Task<TagSummary> GetTagAsync(ulong guildId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            return await _modixContext
                .Set<TagEntity>()
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.Name == name)
                .AsExpandable()
                .Select(TagSummary.FromEntityProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<TagSummary>> GetSummariesAsync(TagSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            return await _modixContext.Set<TagEntity>()
                .Where(x => x.DeleteActionId == null)
                .FilterTagsBy(criteria)
                .OrderBy(x => x.Name)
                .AsExpandable()
                .Select(TagSummary.FromEntityProjection)
                .ToArrayAsync();
        }

        public Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByUserAsync(ulong guildId, ulong userId)
        {
            return GetSummariesAsync(new TagSearchCriteria()
            {
                GuildId = guildId,
                OwnerUserId = userId,
            });
        }

        public Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByRoleAsync(ulong guildId, ulong roleId)
        {
            return GetSummariesAsync(new TagSearchCriteria()
            {
                GuildId = guildId,
                OwnerRoleId = roleId
            });
        }

        public async Task TransferToUserAsync(ulong guildId, string name, ulong currentUserId, ulong userId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            var tag = await _modixContext
                .Set<TagEntity>()
                .Include(x => x.OwnerRole)
                .Include(x => x.OwnerUser)
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.Name == name)
                .SingleOrDefaultAsync();

            if (tag is null)
                return;

            await EnsureUserCanMaintainTagAsync(tag, currentUserId);

            tag.TransferToUser(userId);

            await _modixContext.SaveChangesAsync();
        }

        public async Task TransferToRoleAsync(ulong guildId, string name, ulong currentUserId, ulong roleId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            var tag = await _modixContext
                .Set<TagEntity>()
                .Include(x => x.OwnerRole)
                .Include(x => x.OwnerUser)
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.Name == name)
                .SingleOrDefaultAsync();

            if (tag is null)
                return;

            await EnsureUserCanMaintainTagAsync(tag, currentUserId);

            tag.TransferToRole(roleId);

            await _modixContext.SaveChangesAsync();
        }

        private async Task EnsureUserCanMaintainTagAsync(TagEntity tag, ulong currentUserId)
        {
            var currentUser = await _userService.GetGuildUserAsync(tag.GuildId, currentUserId);

            if (!await CanTriviallyMaintainTagAsync(currentUser))
            {
                if (tag.OwnerUser is null)
                {
                    if (!await CanUserMaintainTagOwnedByRoleAsync(currentUser, tag.OwnerRole))
                        throw new InvalidOperationException("User rank insufficient to transfer the tag.");
                }
                else if (currentUserId != tag.OwnerUser.UserId)
                {
                    throw new InvalidOperationException("User does not own the tag.");
                }
            }
        }

        private async Task<bool> CanUserMaintainTagOwnedByRoleAsync(IGuildUser currentUser, GuildRoleEntity ownerRole)
        {
            Debug.Assert(!(ownerRole is null));

            var rankRoles = await GetRankRolesAsync(currentUser.GuildId);

            // If the owner role is no longer ranked, everything outranks it.
            if (!rankRoles.Any(x => x.Id == ownerRole.RoleId))
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
            if (await _authorizationService.HasClaimsAsync(currentUser.Id, currentUser.Guild.Id, currentUser.RoleIds.ToList(), AuthorizationClaim.MaintainOtherUserTag))
                return true;

            return false;
        }

        private async Task<IEnumerable<GuildRoleBrief>> GetRankRolesAsync(ulong guildId)
            => (await _designatedRoleMappingRepository
                .SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = guildId,
                    Type = DesignatedRoleType.Rank,
                    IsDeleted = false,
                }))
                .Select(r => r.Role);

        public async Task<bool> TagExistsAsync(ulong guildId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            return await _modixContext
                .Set<TagEntity>()
                .Where(x => x.GuildId == guildId)
                .Where(x => x.Name == name)
                .Where(x => x.DeleteActionId == null)
                .AnyAsync();
        }
    }
}
