namespace Discord
{
    /// <inheritdoc cref="IntegrationAccount" />
    public interface IIntegrationAccount
    {
        /// <inheritdoc cref="IntegrationAccount.Id" />
        string Id { get; }

        /// <inheritdoc cref="IntegrationAccount.Name" />
        string Name { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Discord.IntegrationAccount"/>, through the <see cref="IIntegrationAccount"/> interface.
    /// </summary>
    internal struct IntegrationAccountAbstraction : IIntegrationAccount
    {
        /// <summary>
        /// Constructs a new <see cref="IntegrationAccountAbstraction"/> around an existing <see cref="Discord.IntegrationAccount"/>.
        /// </summary>
        /// <param name="integrationAccount">The existing <see cref="Discord.IntegrationAccount"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="integrationAccount"/>.</exception>
        public IntegrationAccountAbstraction(IntegrationAccount integrationAccount)
        {
            _integrationAccount = integrationAccount;
        }

        /// <inheritdoc />
        public string Id
            => _integrationAccount.Id;

        /// <inheritdoc />
        public string Name
            => _integrationAccount.Name;

        /// <inheritdoc cref="IntegrationAccount.ToString"/>
        public override string ToString()
            => _integrationAccount.ToString();

        /// <summary>
        /// The existing <see cref="Discord.IntegrationAccount"/> being abstracted.
        /// </summary>
        private readonly IntegrationAccount _integrationAccount;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="IntegrationAccount"/> objects.
    /// </summary>
    internal static class IntegrationAccountAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IntegrationAccount"/> to an abstracted <see cref="IIntegrationAccount"/> value.
        /// </summary>
        /// <param name="integrationAccount">The existing <see cref="IntegrationAccount"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="integrationAccount"/>.</exception>
        /// <returns>An <see cref="IIntegrationAccount"/> that abstracts <paramref name="integrationAccount"/>.</returns>
        public static IIntegrationAccount Abstract(this IntegrationAccount integrationAccount)
            => new IntegrationAccountAbstraction(integrationAccount);
    }
}
