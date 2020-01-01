using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestSelfUser" />
    public interface IRestSelfUser : IRestUser, ISelfUser { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestSelfUser"/>, through the <see cref="IRestSelfUser"/> interface.
    /// </summary>
    internal class RestSelfUserAbstraction : RestUserAbstraction, IRestSelfUser
    {
        /// <summary>
        /// Constructs a new <see cref="RestSelfUserAbstraction"/> around an existing <see cref="Rest.RestSelfUser"/>.
        /// </summary>
        /// <param name="restSelfUser">The value to use for <see cref="Rest.RestSelfUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restSelfUser"/>.</exception>
        public RestSelfUserAbstraction(RestSelfUser restSelfUser)
            : base(restSelfUser) { }

        /// <inheritdoc />
        public string Email
            => RestSelfUser.Email;

        /// <inheritdoc />
        public bool IsVerified
            => RestSelfUser.IsVerified;

        /// <inheritdoc />
        public bool IsMfaEnabled
            => RestSelfUser.IsMfaEnabled;

        /// <inheritdoc />
        public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions options = null)
            => RestSelfUser.ModifyAsync(func, options);

        /// <summary>
        /// The existing <see cref="Rest.RestSelfUser"/> being abstracted.
        /// </summary>
        protected RestSelfUser RestSelfUser
            => RestUser as RestSelfUser;

        public UserProperties Flags => throw new NotImplementedException();

        public PremiumType PremiumType => throw new NotImplementedException();

        public string Locale => throw new NotImplementedException();
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestSelfUser"/> objects.
    /// </summary>
    internal static class RestSelfUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestSelfUser"/> to an abstracted <see cref="IRestSelfUser"/> value.
        /// </summary>
        /// <param name="restSelfUser">The existing <see cref="RestSelfUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restSelfUser"/>.</exception>
        /// <returns>An <see cref="IRestSelfUser"/> that abstracts <paramref name="restSelfUser"/>.</returns>
        public static IRestSelfUser Abstract(this RestSelfUser restSelfUser)
            => new RestSelfUserAbstraction(restSelfUser);
    }
}
