namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Defines the possible types of actions that can be performed on tags.
    /// </summary>
    public enum TagActionType
    {
        /// <summary>
        /// Describes an action where a tag was created.
        /// </summary>
        TagCreated,
        /// <summary>
        /// Describes an action where a tag was modified.
        /// </summary>
        TagModified,
        /// <summary>
        /// Describes an action where a tag was deleted.
        /// </summary>
        TagDeleted,
    }
}
