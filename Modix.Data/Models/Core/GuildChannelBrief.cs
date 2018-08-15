namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a partial view of an <see cref="GuildChannelEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class GuildChannelBrief
    {
        /// <summary>
        /// See <see cref="GuildChannelEntity.ChannelId"/>.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// See <see cref="GuildChannelEntity.Name"/>.
        /// </summary>
        public string Name { get; set; }
    }
}
