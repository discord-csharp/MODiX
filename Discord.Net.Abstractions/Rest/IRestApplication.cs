using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestApplication" />
    public interface IRestApplication : IApplication
    {
        /// <inheritdoc cref="RestApplication.UpdateAsync" />
        Task UpdateAsync();
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestApplication"/>, through the <see cref="IRestApplication"/> interface.
    /// </summary>
    public class RestApplicationAbstraction : IRestApplication
    {
        /// <summary>
        /// Constructs a new <see cref="RestApplicationAbstraction"/> around an existing <see cref="Rest.RestApplication"/>.
        /// </summary>
        /// <param name="restApplication">The value to use for <see cref="Rest.RestApplication"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restApplication"/>.</exception>
        public RestApplicationAbstraction(RestApplication restApplication)
        {
            RestApplication = restApplication ?? throw new ArgumentNullException(nameof(restApplication));
        }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestApplication.CreatedAt;

        /// <inheritdoc />
        public string Description
            => RestApplication.Description;

        /// <inheritdoc />
        public ulong Flags
            => RestApplication.Flags;

        /// <inheritdoc />
        public string IconUrl
            => RestApplication.IconUrl;

        /// <inheritdoc />
        public ulong Id
            => RestApplication.Id;

        /// <inheritdoc />
        public string Name
            => RestApplication.Name;

        /// <inheritdoc />
        public IUser Owner
            => RestApplication.Owner
                .Abstract();

        /// <inheritdoc />
        public string[] RPCOrigins
            => RestApplication.RPCOrigins;

        /// <inheritdoc />
        public Task UpdateAsync()
            => RestApplication.UpdateAsync();

        /// <inheritdoc cref="RestApplication.ToString" />
        public override string ToString()
            => RestApplication.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestApplication"/> being abstracted.
        /// </summary>
        protected RestApplication RestApplication { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestApplication"/> objects.
    /// </summary>
    public static class RestApplicationAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestApplication"/> to an abstracted <see cref="IRestApplication"/> value.
        /// </summary>
        /// <param name="restApplication">The existing <see cref="RestApplication"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restApplication"/>.</exception>
        /// <returns>An <see cref="IRestApplication"/> that abstracts <paramref name="restApplication"/>.</returns>
        public static IRestApplication Abstract(this RestApplication restApplication)
            => new RestApplicationAbstraction(restApplication);
    }
}
