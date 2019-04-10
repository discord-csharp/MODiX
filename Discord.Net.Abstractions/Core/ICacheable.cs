using System;
using System.Threading.Tasks;

namespace Discord
{
    /// <inheritdoc cref="Cacheable{TEntity, TId}" />
    public interface ICacheable<TEntity, TId>
        where TEntity : IEntity<TId>
        where TId : IEquatable<TId>
    {
        /// <inheritdoc cref="Cacheable{TEntity, TId}.HasValue" />
        bool HasValue { get; }

        /// <inheritdoc cref="Cacheable{TEntity, TId}.Id" />
        TId Id { get; }

        /// <inheritdoc cref="Cacheable{TEntity, TId}.Value" />
        TEntity Value { get; }

        /// <inheritdoc cref="Cacheable{TEntity, TId}.DownloadAsync" />
        Task<TEntity> DownloadAsync();

        /// <inheritdoc cref="Cacheable{TEntity, TId}.GetOrDownloadAsync" />
        Task<TEntity> GetOrDownloadAsync();
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Discord.Cacheable{TEntity, TId}"/>, through the <see cref="ICacheable{TEntity, TId}"/> interface.
    /// </summary>
    public struct CacheableAbstraction<TEntity, TId> : ICacheable<TEntity, TId>
        where TEntity : IEntity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Constructs a new <see cref="CacheableAbstraction{TEntity, TId}"/> around an existing <see cref="Discord.Cacheable{TEntity, TId}"/>.
        /// </summary>
        /// <param name="cacheable">The existing <see cref="Discord.Cacheable{TEntity, TId}"/> to be abstracted.</param>
        public CacheableAbstraction(Cacheable<TEntity, TId> cacheable)
        {
            _cacheable = cacheable;
        }

        /// <inheritdoc />
        public bool HasValue
            => _cacheable.HasValue;

        /// <inheritdoc />
        public TId Id
            => _cacheable.Id;

        /// <inheritdoc />
        public TEntity Value
            => (TEntity)AbstractEntity(_cacheable.Value);

        /// <inheritdoc />
        public async Task<TEntity> DownloadAsync()
            => (TEntity)AbstractEntity(await _cacheable.DownloadAsync());

        /// <inheritdoc />
        public async Task<TEntity> GetOrDownloadAsync()
           => (TEntity)AbstractEntity(await _cacheable.GetOrDownloadAsync());

        private static object AbstractEntity(object entity)
            => entity switch
            {
                IUserMessage userMessage
                    => userMessage.Abstract() as object,
                IMessage message
                    => message.Abstract() as object,
                IGuild guild
                    => guild.Abstract() as object,
                _
                    => throw new NotSupportedException($"Caching is not supported for Discord type {entity.GetType().FullName}")
            };

        private readonly Cacheable<TEntity, TId> _cacheable;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="Cacheable{TEntity, TId}"/> objects.
    /// </summary>
    public static class CacheableAsbtractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="Cacheable{TEntity, TId}"/> to an abstracted <see cref="ICacheable{TEntity, TId}"/> value.
        /// </summary>
        /// <param name="cacheable">The existing <see cref="Cacheable{TEntity, TId}"/> to be abstracted.</param>
        /// <returns>An <see cref="ICacheable{TEntity, TId}"/> that abstracts <paramref name="cacheable"/>.</returns>
        public static ICacheable<TEntity, TId> Abstract<TEntity, TId>(this Cacheable<TEntity, TId> cacheable)
                where TEntity : IEntity<TId>
                where TId : IEquatable<TId>
            => new CacheableAbstraction<TEntity, TId>(cacheable);
    }
}
