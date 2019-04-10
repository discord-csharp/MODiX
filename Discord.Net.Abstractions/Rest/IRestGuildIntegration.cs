using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestGuildIntegration" />
    public interface IRestGuildIntegration : IEntity<ulong>, IGuildIntegration
    {
        /// <inheritdoc cref="RestGuildIntegration.User" />
        new IRestUser User { get; }

        /// <inheritdoc cref="RestGuildIntegration.Account" />
        new IIntegrationAccount Account { get; }

        /// <inheritdoc cref="RestGuildIntegration.DeleteAsync" />
        Task DeleteAsync();

        /// <inheritdoc cref="RestGuildIntegration.ModifyAsync(Action{GuildIntegrationProperties})" />
        Task ModifyAsync(Action<GuildIntegrationProperties> func);

        /// <inheritdoc cref="RestGuildIntegration.SyncAsync" />
        Task SyncAsync();
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestGuildIntegration"/>, through the <see cref="IRestGuildIntegration"/> interface.
    /// </summary>
    public class RestGuildIntegrationAbstraction : IRestGuildIntegration
    {
        /// <summary>
        /// Constructs a new <see cref="RestGuildIntegrationAbstraction"/> around an existing <see cref="Rest.RestGuildIntegration"/>.
        /// </summary>
        /// <param name="restGuildIntegration">The value to use for <see cref="Rest.RestGuildIntegration"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGuildIntegration"/>.</exception>
        public RestGuildIntegrationAbstraction(RestGuildIntegration restGuildIntegration)
        {
            RestGuildIntegration = restGuildIntegration ?? throw new ArgumentNullException(nameof(restGuildIntegration));
        }

        /// <inheritdoc />
        public IIntegrationAccount Account
            => RestGuildIntegration.Account
                .Abstract();

        /// <inheritdoc />
        IntegrationAccount IGuildIntegration.Account
            => RestGuildIntegration.Account;

        /// <inheritdoc />
        public ulong Id
            => RestGuildIntegration.Id;

        /// <inheritdoc />
        public ulong ExpireBehavior
            => RestGuildIntegration.ExpireBehavior;

        /// <inheritdoc />
        public ulong ExpireGracePeriod
            => RestGuildIntegration.ExpireGracePeriod;

        /// <inheritdoc />
        public bool IsEnabled
            => RestGuildIntegration.IsEnabled;

        /// <inheritdoc />
        public bool IsSyncing
            => RestGuildIntegration.IsSyncing;

        /// <inheritdoc />
        public IGuild Guild
            => (RestGuildIntegration as IGuildIntegration).Guild
                .Abstract();

        /// <inheritdoc />
        public ulong GuildId
            => RestGuildIntegration.GuildId;

        /// <inheritdoc />
        public string Name
            => RestGuildIntegration.Name;

        /// <inheritdoc />
        public ulong RoleId
            => RestGuildIntegration.RoleId;

        /// <inheritdoc />
        public DateTimeOffset SyncedAt
            => RestGuildIntegration.SyncedAt;

        /// <inheritdoc />
        public string Type
            => RestGuildIntegration.Type;

        /// <inheritdoc />
        public IRestUser User
            => RestGuildIntegration.User
                .Abstract();

        /// <inheritdoc />
        IUser IGuildIntegration.User
            => (RestGuildIntegration as IGuildIntegration).User
                .Abstract();

        /// <inheritdoc />
        public Task DeleteAsync()
            => RestGuildIntegration.DeleteAsync();

        /// <inheritdoc />
        public Task ModifyAsync(Action<GuildIntegrationProperties> func)
            => RestGuildIntegration.ModifyAsync(func);

        /// <inheritdoc />
        public Task SyncAsync()
            => RestGuildIntegration.SyncAsync();

        /// <inheritdoc cref="RestGuildIntegration.ToString" />
        public override string ToString()
            => RestGuildIntegration.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestGuildIntegration"/> being abstracted.
        /// </summary>
        protected RestGuildIntegration RestGuildIntegration { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestGuildIntegration"/> objects.
    /// </summary>
    public static class RestGuildIntegrationAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestGuildIntegration"/> to an abstracted <see cref="IRestGuildIntegration"/> value.
        /// </summary>
        /// <param name="restGuildIntegration">The existing <see cref="RestGuildIntegration"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGuildIntegration"/>.</exception>
        /// <returns>An <see cref="IRestGuildIntegration"/> that abstracts <paramref name="restGuildIntegration"/>.</returns>
        public static IRestGuildIntegration Abstract(this RestGuildIntegration restGuildIntegration)
            => new RestGuildIntegrationAbstraction(restGuildIntegration);
    }
}
