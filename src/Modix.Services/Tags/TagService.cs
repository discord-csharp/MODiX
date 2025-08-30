using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Modix.Data;
using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;
using Modix.Data.Repositories;
using Modix.Services.Core;

namespace Modix.Services.Tags
{
    public interface ITagService
    {
        Task CreateTagAsync(ulong guildId, ulong creatorId, string name, string content);

        Task UseTagAsync(ulong guildId, ulong channelId, string name, IMessage invokingMessage);

        Task ModifyTagAsync(ulong guildId, ulong modifierId, string name, string newContent);

        Task DeleteTagAsync(ulong guildId, ulong deleterId, string name);

        Task<TagSummary> GetTagAsync(ulong guildId, string name);

        Task<IReadOnlyCollection<TagSummary>> GetSummariesAsync(TagSearchCriteria criteria);

        Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByUserAsync(ulong guildId, ulong userId);

        Task<IReadOnlyCollection<TagSummary>> GetTagsOwnedByRoleAsync(ulong guildId, ulong roleId);

        Task TransferToUserAsync(ulong guildId, string name, ulong currentUserId, ulong userId);

        Task TransferToRoleAsync(ulong guildId, string name, ulong currentUserId, ulong roleId);

        Task<bool> TagExistsAsync(ulong guildId, string name);

        Task RefreshCache(ulong guildId);

        Task EnsureUserCanMaintainTagAsync(ulong guildId, string name, ulong currentUserId);
    }

    internal class TagService : ITagService
    {
        private readonly IDiscordClient _discordClient;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserService _userService;
        private readonly IDesignatedRoleMappingRepository _designatedRoleMappingRepository;
        private readonly ITagCache _tagCache;
        private readonly ModixContext _modixContext;

        private static readonly Regex _tagNameRegex = new(@"^\S+\b$");

        public TagService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IUserService userService,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository,
            ITagCache tagCache,
            ModixContext modixContext)
        {
            _discordClient = discordClient;
            _authorizationService = authorizationService;
            _userService = userService;
            _modixContext = modixContext;
            _designatedRoleMappingRepository = designatedRoleMappingRepository;
            _tagCache = tagCache;
        }

#nullable enable
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
                Name = name,
                Content = content,
            };

            var guild = await _discordClient.GetGuildAsync(guildId);
            var creator = await guild.GetUserAsync(creatorId);
            var defaultOwnerRole = await GetDefaultOwnerRoleAsync(creator);

            if (defaultOwnerRole is not null)
                tag.OwnerRoleId = defaultOwnerRole.Id;
            else
                tag.OwnerUserId = creatorId;

            var createAction = new TagActionEntity()
            {
                GuildId = guildId,
                Created = DateTimeOffset.UtcNow,
                Type = TagActionType.TagCreated,
                CreatedById = creatorId,
            };

            tag.CreateAction = createAction;

            _modixContext.Set<TagEntity>().Add(tag);

            await _modixContext.SaveChangesAsync();

            _tagCache.Add(guildId, name);
        }
#nullable restore

        public async Task UseTagAsync(ulong guildId, ulong channelId, string name, IMessage invokingMessage)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.UseTag);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            name = name.Trim().ToLower();

            var channel = await _discordClient.GetChannelAsync(channelId);

            if (channel is not IMessageChannel messageChannel)
                throw new InvalidOperationException($"The channel '{channel.Name}' is not a message channel.");

            var tag = await _modixContext
                .Set<TagEntity>()
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.Name == name)
                .SingleOrDefaultAsync();

            if (tag is null)
                return;

            await messageChannel.SendMessageAsync(tag.Content, messageReference: new(invokingMessage.Id), allowedMentions: AllowedMentions.None);

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
                throw new ArgumentException("The tag provided was not found.");

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
                throw new ArgumentException("The tag provided was not found.");

            await EnsureUserCanMaintainTagAsync(tag, deleterId);

            tag.Delete(deleterId);

            await _modixContext.SaveChangesAsync();

            _tagCache.Remove(guildId, name);
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

        public async Task RefreshCache(ulong guildId)
        {
            var tags = await _modixContext
                .Set<TagEntity>()
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Select(x => x.Name)
                .ToArrayAsync();

            _tagCache.Set(guildId, tags);
        }

        public async Task EnsureUserCanMaintainTagAsync(ulong guildId, string name, ulong currentUserId)
        {
            var tag = await _modixContext
                .Set<TagEntity>()
                .Include(x => x.OwnerRole)
                .Include(x => x.OwnerUser)
                .Where(x => x.GuildId == guildId)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.Name == name)
                .SingleOrDefaultAsync();

            await EnsureUserCanMaintainTagAsync(tag, currentUserId);
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
            Debug.Assert(ownerRole is not null);

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

#nullable enable
        private async Task<IRole?> GetDefaultOwnerRoleAsync(IGuildUser user)
        {
            var lowestRankRole = (await _designatedRoleMappingRepository.SearchBriefsAsync(new()
            {
                GuildId = user.GuildId,
                Type = DesignatedRoleType.Rank,
                IsDeleted = false,
                RoleIds = user.RoleIds.ToArray(),
            }))
            .OrderBy(x => x.Role.Position)
            .FirstOrDefault();

            if (lowestRankRole is null)
                return null;

            return user.Guild.Roles.FirstOrDefault(x => x.Id == lowestRankRole.Role.Id);
        }
#nullable restore

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
    }
}
