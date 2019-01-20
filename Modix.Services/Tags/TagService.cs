using System;
using System.Collections.Generic;
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
    }

    /// <inheritdoc />
    internal class TagService : ITagService
    {
        /// <summary>
        /// Constructs a new <see cref="TagService"/> with the supplied dependencies.
        /// </summary>
        public TagService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            ITagRepository tagRepository)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            TagRepository = tagRepository;
        }

        /// <inheritdoc />
        public async Task CreateTagAsync(ulong guildId, ulong creatorId, string name, string content)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.CreateTag);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The tag name cannot be blank or whitespace.", nameof(name));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("The tag content cannot be blank or whitespace.", nameof(content));

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

                var sanitizedContent = FormatUtilities.Sanitize(tag.Content);

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

                if (tag.CreateAction.CreatedBy.Id != modifierId)
                    AuthorizationService.RequireClaims(AuthorizationClaim.MaintainOtherUserTag);

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

                if (tag.CreateAction.CreatedBy.Id != deleterId)
                    AuthorizationService.RequireClaims(AuthorizationClaim.MaintainOtherUserTag);

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
    }
}
