using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestRole" />
    public interface IRestRole : IRole
    {
        /// <inheritdoc cref="RestRole.IsEveryone" />
        bool IsEveryone { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestRole"/>, through the <see cref="IRestRole"/> interface.
    /// </summary>
    public class RestRoleAbstraction : IRestRole
    {
        /// <summary>
        /// Constructs a new <see cref="RestRoleAbstraction"/> around an existing <see cref="Rest.RestRole"/>.
        /// </summary>
        /// <param name="restRole">The value to use for <see cref="Rest.RestRole"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restRole"/>.</exception>
        public RestRoleAbstraction(RestRole restRole)
        {
            RestRole = restRole ?? throw new ArgumentNullException(nameof(restRole));
        }

        /// <inheritdoc />
        public Color Color
            => RestRole.Color;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestRole.CreatedAt;

        /// <inheritdoc />
        public IGuild Guild
            => (RestRole as IRole).Guild;

        /// <inheritdoc />
        public ulong Id
            => RestRole.Id;

        /// <inheritdoc />
        public bool IsEveryone
            => RestRole.IsEveryone;

        /// <inheritdoc />
        public bool IsHoisted
            => RestRole.IsHoisted;

        /// <inheritdoc />
        public bool IsManaged
            => RestRole.IsManaged;

        /// <inheritdoc />
        public bool IsMentionable
            => RestRole.IsMentionable;

        /// <inheritdoc />
        public string Mention
            => RestRole.Mention;

        /// <inheritdoc />
        public string Name
            => RestRole.Name;

        /// <inheritdoc />
        public GuildPermissions Permissions
            => RestRole.Permissions;

        /// <inheritdoc />
        public int Position
            => RestRole.Position;

        /// <inheritdoc />
        public Task ModifyAsync(Action<RoleProperties> func, RequestOptions options = null)
            => RestRole.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => RestRole.DeleteAsync(options);

        /// <inheritdoc />
        public int CompareTo(IRole other)
            => RestRole.CompareTo(other);

        /// <inheritdoc cref="RestRole.ToString" />
        public override string ToString()
            => RestRole.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestRole"/> being abstracted.
        /// </summary>
        protected RestRole RestRole { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestRole"/> objects.
    /// </summary>
    public static class RestRoleAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestRole"/> to an abstracted <see cref="IRestRole"/> value.
        /// </summary>
        /// <param name="restRole">The existing <see cref="RestRole"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restRole"/>.</exception>
        /// <returns>An <see cref="IRestRole"/> that abstracts <paramref name="restRole"/>.</returns>
        public static IRestRole Abstract(this RestRole restRole)
            => new RestRoleAbstraction(restRole);
    }
}
