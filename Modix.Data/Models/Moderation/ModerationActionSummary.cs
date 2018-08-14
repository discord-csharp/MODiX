using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a <see cref="ModerationActionEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ModerationActionSummary
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Type"/>.
        /// </summary>
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedBy"/>.
        /// </summary>
        public GuildUserIdentity CreatedBy { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Infraction"/>.
        /// </summary>
        public InfractionBrief Infraction { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.DeletedMessage"/>.
        /// </summary>
        public DeletedMessageBrief DeletedMessage { get; set; }

        internal static Expression<Func<ModerationActionEntity, ModerationActionSummary>> FromEntityProjection { get; }
            = entity => new ModerationActionSummary()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                Type = entity.Type,
                Created = entity.Created,
                CreatedBy = new GuildUserIdentity()
                {
                    Id = (ulong)entity.CreatedBy.UserId,
                    Username = entity.CreatedBy.User.Username,
                    Discriminator = entity.CreatedBy.User.Discriminator,
                    Nickname = entity.CreatedBy.Nickname
                },
                Infraction = (entity.InfractionId == null) ? null : new InfractionBrief()
                {
                    Id = entity.Infraction.Id,
                    // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                    //Type = entity.Infraction.Type,
                    Type = Enum.Parse<InfractionType>(entity.Infraction.Type.ToString()),
                    Reason = entity.Infraction.Reason,
                    Duration = entity.Infraction.Duration,
                    Subject = new GuildUserIdentity()
                    {
                        Id = (ulong)entity.Infraction.Subject.UserId,
                        Username = entity.Infraction.Subject.User.Username,
                        Discriminator = entity.Infraction.Subject.User.Discriminator,
                        Nickname = entity.Infraction.Subject.Nickname
                    },
                },
                DeletedMessage = (entity.DeletedMessageId == null) ? null : new DeletedMessageBrief()
                {
                    Id = entity.DeletedMessage.MessageId,
                    Channel = new GuildChannelBrief()
                    {
                        Id = (ulong)entity.DeletedMessage.Channel.ChannelId,
                        Name = entity.DeletedMessage.Channel.Name,
                    },
                    Author = new GuildUserIdentity()
                    {
                        Id = (ulong)entity.DeletedMessage.Author.UserId,
                        Username = entity.DeletedMessage.Author.User.Username,
                        Discriminator = entity.DeletedMessage.Author.User.Discriminator,
                        Nickname = entity.DeletedMessage.Author.Nickname
                    },
                    Content = entity.DeletedMessage.Content,
                    Reason = entity.DeletedMessage.Reason,
                }
            };
    }
}
