using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestUserGuild" />
    public interface IRestUserGuild : IUserGuild, IDeletable, ISnowflakeEntity, IEntity<ulong>
    {
        /// <inheritdoc cref="RestUserGuild.LeaveAsync(RequestOptions)" />
        Task LeaveAsync(RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestUserGuild"/>, through the <see cref="IRestUserGuild"/> interface.
    /// </summary>
    public class RestUserGuildAbstraction : IRestUserGuild
    {
        /// <summary>
        /// Constructs a new <see cref="RestUserGuildAbstraction"/> around an existing <see cref="Rest.RestUserGuild"/>.
        /// </summary>
        /// <param name="restUserGuild">The value to use for <see cref="Rest.RestUserGuild"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restUserGuild"/>.</exception>
        public RestUserGuildAbstraction(RestUserGuild restUserGuild)
        {
            if (restUserGuild is null)
                throw new ArgumentNullException(nameof(restUserGuild));

            RestUserGuild = restUserGuild;
        }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestUserGuild.CreatedAt;

        /// <inheritdoc />
        public string IconUrl
            => RestUserGuild.IconUrl;

        /// <inheritdoc />
        public ulong Id
            => RestUserGuild.Id;

        /// <inheritdoc />
        public bool IsOwner
            => RestUserGuild.IsOwner;

        /// <inheritdoc />
        public string Name
            => RestUserGuild.Name;

        /// <inheritdoc />
        public GuildPermissions Permissions
            => RestUserGuild.Permissions;

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => RestUserGuild.DeleteAsync(options);

        /// <inheritdoc />
        public Task LeaveAsync(RequestOptions options = null)
            => RestUserGuild.LeaveAsync(options);

        /// <inheritdoc cref="RestUserGuild.ToString" />
        public override string ToString()
            => RestUserGuild.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestUserGuild"/> being abstracted.
        /// </summary>
        protected RestUserGuild RestUserGuild { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestUserGuild"/> objects.
    /// </summary>
    public static class RestUserGuildAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestUserGuild"/> to an abstracted <see cref="IRestUserGuild"/> value.
        /// </summary>
        /// <param name="restUserGuild">The existing <see cref="RestUserGuild"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restUserGuild"/>.</exception>
        /// <returns>An <see cref="IRestUserGuild"/> that abstracts <paramref name="restUserGuild"/>.</returns>
        public static IRestUserGuild Abstract(this RestUserGuild restUserGuild)
            => new RestUserGuildAbstraction(restUserGuild);
    }
}
