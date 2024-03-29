using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Tags
{
    public interface ITagCache
    {
        void Add(ulong guildId, string tagName);
        void Remove(ulong guildId, string tagName);
        void Set(ulong guildId, IEnumerable<string> tags);
        ImmutableArray<string> Search(ulong guildId, string partialName, int? maxResults = null);
    }

    [ServiceBinding(ServiceLifetime.Singleton)]
    internal class TagCache : ITagCache
    {
        private readonly IMemoryCache _cache;

        private static readonly ReaderWriterLockSlim _cacheLock = new();

        /// <summary>
        /// For performance reasons, this class has as few dependencies as possible.
        /// </summary>
        public TagCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Add(ulong guildId, string tagName)
        {
            _cacheLock.EnterWriteLock();

            try
            {
                var cachedTags = GetFromCache(guildId);
                cachedTags.Add(tagName);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        public void Remove(ulong guildId, string tagName)
        {
            _cacheLock.EnterWriteLock();

            try
            {
                var cachedTags = GetFromCache(guildId);
                cachedTags.Remove(tagName);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        public void Set(ulong guildId, IEnumerable<string> tags)
        {
            _cacheLock.EnterWriteLock();

            try
            {
                _cache.Set(GetCacheKey(guildId), new SortedSet<string>(tags));
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        public ImmutableArray<string> Search(ulong guildId, string partialName, int? maxResults = null)
        {
            if (string.IsNullOrWhiteSpace(partialName))
                return ImmutableArray<string>.Empty;

            _cacheLock.EnterReadLock();

            try
            {
                var cachedTags = GetFromCache(guildId);

                IEnumerable<string> results = cachedTags
                    .Where(x => x.Contains(partialName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => x);

                if (maxResults is not null)
                {
                    results = results.Take(maxResults.GetValueOrDefault());
                }

                return results.ToImmutableArray();
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        private SortedSet<string> GetFromCache(ulong guildId)
            => _cache.Get<SortedSet<string>>(GetCacheKey(guildId)) ?? new();

        private static object GetCacheKey(ulong guildId)
            => new { guildId, Target = "Tags" };
    }
}
