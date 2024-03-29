namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a basic repository, which acts as a gatekeeper for all data storage and data retrieval operations,
    /// involving a particular type of data or record, and performs all interaction with the data storage layer.
    /// </summary>
    public abstract class RepositoryBase
    {
        /// <summary>
        /// Creates a new <see cref="InfractionRepository"/>.
        /// </summary>
        /// <param name="modixContext">The value to use for <see cref="ModixContext"/>.</param>
        public RepositoryBase(ModixContext modixContext)
        {
            ModixContext = modixContext;
        }

        /// <summary>
        /// The <see cref="Data.ModixContext"/> to be used to interact with the data storage layer.
        /// </summary>
        internal protected ModixContext ModixContext { get; }
    }
}
