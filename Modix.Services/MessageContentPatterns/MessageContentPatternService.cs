using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.MessageContentPatterns
{
    public interface IMessageContentPatternService
    {
        bool CanViewPatterns();
        Task<List<MessageContentPatternDto>> GetPatterns(ulong guildId);
        Task<ServiceResponse> AddPattern(ulong guildId, string regexPattern, MessageContentPatternType patternType);
        Task<ServiceResponse> RemovePattern(ulong guildId, string regexPattern);
    }

    [ServiceBinding(ServiceLifetime.Scoped)]
    public class MessageContentPatternService : IMessageContentPatternService
    {
        private readonly ModixContext _db;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMemoryCache _memoryCache;

        public MessageContentPatternService(ModixContext db, IAuthorizationService authorizationService, IMemoryCache memoryCache)
        {
            _db = db;
            _authorizationService = authorizationService;
            _memoryCache = memoryCache;
        }

        private static object GetKeyForCache(ulong guildId) => new {guildId, Target = "MessageContentPattern"};

        private readonly MemoryCacheEntryOptions _patternCacheEntryOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));

        public bool CanViewPatterns() =>
            _authorizationService.HasClaim(AuthorizationClaim.ManageMessageContentPatterns);

        public async Task<List<MessageContentPatternDto>> GetPatterns(ulong guildId)
        {
            var key = GetKeyForCache(guildId);

            if (!_memoryCache.TryGetValue(key, out List<MessageContentPatternDto> patterns))
            {
                patterns = await _db
                    .Set<MessageContentPatternEntity>()
                    .Where(x => x.GuildId == guildId)
                    .Select(x => new MessageContentPatternDto(x.Pattern, x.PatternType))
                    .ToListAsync();

                _memoryCache.Set(key, patterns, _patternCacheEntryOptions);
            }

            return patterns;
        }

        public async Task<ServiceResponse> AddPattern(ulong guildId, string regexPattern, MessageContentPatternType patternType)
        {
            if (!_authorizationService.HasClaim(AuthorizationClaim.ManageMessageContentPatterns))
            {
                return ServiceResponse.Fail("User does not have claim to manage patterns!");
            }

            if (!IsValidRegex(regexPattern))
            {
                return ServiceResponse.Fail("Pattern is not a valid Regex!");
            }

            if (await DoesPatternExist(guildId, regexPattern))
            {
                return ServiceResponse.Fail("Pattern already exists!");
            }

            var entity = new MessageContentPatternEntity
            {
                GuildId = guildId,
                Pattern = regexPattern,
                PatternType = patternType,
            };

            _db.Add(entity);

            await _db.SaveChangesAsync();

            ClearCacheForGuild(guildId);

            return ServiceResponse.Ok();
        }

        private bool IsValidRegex(string candidate)
        {
            try
            {
                _ = new Regex(candidate);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<ServiceResponse> RemovePattern(ulong guildId, string regexPattern)
        {
            if (!_authorizationService.HasClaim(AuthorizationClaim.ManageMessageContentPatterns))
            {
                return ServiceResponse.Fail("User does not have claim to manage patterns!");
            }

            var pattern = await _db
                .Set<MessageContentPatternEntity>()
                .Where(x => x.GuildId == guildId)
                .Where(x => x.Pattern == regexPattern)
                .SingleOrDefaultAsync();

            if (!(pattern is null))
            {
                _db.Remove(pattern);

                await _db.SaveChangesAsync();
            }

            ClearCacheForGuild(guildId);

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
            var cacheKey = GetKeyForCache(guildId);
            _memoryCache.Remove(cacheKey);
        }
    }
}
