using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Data.Utilities;
using Modix.Services.Core;
using Modix.Services.MessageContentPatterns;
using Modix.Services.Utilities;

namespace Modix.Services.ListeningMessagePatterns
{
    public interface IListeningMessagePatternService
    {
        Task DisablePattern(ulong guildId, string regexPattern);
        Task<Regex> GetGuildCatchAllPatternAsync(ulong guildId);
        Task<List<ListeningPatternDto>> GetGuildPatternsAsync(ulong guildId);
    }

    [ServiceBinding(ServiceLifetime.Scoped)]
    public class ListeningMessagePatternService : IListeningMessagePatternService
    {
        private readonly ModixContext _db;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMemoryCache _memoryCache;

        public ListeningMessagePatternService(ModixContext db, IAuthorizationService authorizationService, IMemoryCache memoryCache)
        {
            _db = db;
            _authorizationService = authorizationService;
            _memoryCache = memoryCache;
        }

        private static object GetKeyForCatchAllPatternCache(ulong guildId) => new { guildId, Target = "GuildListeningPattern" };
        private static object GetKeyForGuildPatternsCache(ulong guildId) => new { guildId, Target = "ListeningPatternEntries" };

        private readonly MemoryCacheEntryOptions _patternCacheEntryOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));

        public async Task<List<ListeningPatternDto>> GetGuildPatternsAsync(ulong guildId)
        {
            var key = GetKeyForGuildPatternsCache(guildId);

            if (!_memoryCache.TryGetValue(key, out List<ListeningPatternDto> patterns))
            {
                patterns = await _db
                    .Set<ListeningMessagePatternEntity>()
                    .Where(x => x.GuildId == guildId && !x.Disabled)
                    .Select(x => new ListeningPatternDto(x.Pattern))
                    .ToListAsync();

                _memoryCache.Set(key, patterns, _patternCacheEntryOptions);
            }
            return patterns;
        }

        public async Task<Regex> GetGuildCatchAllPatternAsync(ulong guildId)
        {
            var key = GetKeyForCatchAllPatternCache(guildId);

            if (_memoryCache.TryGetValue(key, out Regex pattern))
                return pattern;
            var patterns = await _db
                 .Set<ListeningMessagePatternEntity>()
                 .Where(x => x.GuildId == guildId)
                 .Select(x => $"({x.Pattern})")
                 .ToListAsync();

            // We build a regex that match every single pattern of the guild.
            // while the moderation patterns are few, if this feature see lot of usage, we could see hundreds of regex.
            // I suspect the compiled regex will help a lot in reducing the cost

            // yes it's premature optimisation, but it's fun.
            var catchAllPattern = string.Join('|', patterns);
            var regex = new Regex(catchAllPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(5));
            return _memoryCache.Set(key, regex, _patternCacheEntryOptions);
        }

        public async Task DisablePattern(ulong guildId, string regexPattern)
        {
            var pattern = await _db.Set<ListeningMessagePatternEntity>()
                .Where(x => x.GuildId == guildId)
                .Where(x => x.Pattern == regexPattern)
                .SingleAsync();
            pattern.Disabled = true;
            _db.Update(pattern);
            await _db.SaveChangesAsync();
            ClearCacheForGuild(guildId);
        }

        public async Task<ServiceResponse> SubscribeToPattern(ulong guildId, string regexPattern)
        {
            if (!RegexUtilities.IsValidRegex(regexPattern))
            {
                return ServiceResponse.Fail("Pattern is not a valid Regex!");
            }

            TODO
            return ServiceResponse.Ok();
        }

        public async Task<ServiceResponse> UnsubscribeToPattern(ulong guildId, string regexPattern )
        {
            TODO
            return ServiceResponse.Ok();
        }

        private async Task<bool> DoesPatternExist(ulong guildId, string regexPattern)
        {
            return await _db
                .Set<MessageContentPatternEntity>()
                .Where(x => x.GuildId == guildId && x.Pattern == regexPattern)
                .AnyAsync();
        }

        private void ClearCacheForGuild(ulong guildId)
        {
            _memoryCache.Remove(GetKeyForCatchAllPatternCache(guildId));
            _memoryCache.Remove(GetKeyForGuildPatternsCache(guildId));
        }
    }
}
