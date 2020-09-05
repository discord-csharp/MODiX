using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.Blocklist
{
    public interface IMessageContentPatternService
    {
        Task<List<MessageContentPatternDto>> GetPatterns(ulong guildId);
        Task<bool> DoesPatternExist(ulong guildId, string regexPattern);
        Task<ServiceResponse> AddPattern(ulong guildId, string regexPattern, MessageContentPatternType patternType);
        Task<ServiceResponse> RemovePattern(ulong guildId, string regexPattern);
    }

    [ServiceBinding(ServiceLifetime.Scoped)]
    public class MessageContentPatternService : IMessageContentPatternService
    {
        private readonly ModixContext _db;
        private readonly IAuthorizationService _authorizationService;

        public MessageContentPatternService(ModixContext db, IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }

        public async Task<List<MessageContentPatternDto>> GetPatterns(ulong guildId)
        {
            return await _db
                .Set<MessageContentPatternEntity>()
                .Where(x => x.GuildId == guildId)
                .Select(x => new MessageContentPatternDto(x.Pattern, x.PatternType))
                .ToListAsync();
        }

        public async Task<bool> DoesPatternExist(ulong guildId, string regexPattern)
        {
            return await _db
                .Set<MessageContentPatternEntity>()
                .Where(x => x.GuildId == guildId && x.Pattern == regexPattern)
                .AnyAsync();
        }

        public async Task<ServiceResponse> AddPattern(ulong guildId, string regexPattern, MessageContentPatternType patternType)
        {
            if (!_authorizationService.HasClaim(AuthorizationClaim.ManageMessageContentPatterns))
            {
                ServiceResponse.Fail("User does not have claim to manage patterns!");
            }

            if (await DoesPatternExist(guildId, regexPattern))
            {
                ServiceResponse.Fail("Pattern already exists!");
            }

            var entity = new MessageContentPatternEntity
            {
                GuildId = guildId,
                Pattern = regexPattern,
                PatternType = patternType,
            };

            _db.Add(entity);

            await _db.SaveChangesAsync();

            return ServiceResponse.Ok();
        }

        public async Task<ServiceResponse> RemovePattern(ulong guildId, string regexPattern)
        {
            if (!_authorizationService.HasClaim(AuthorizationClaim.ManageMessageContentPatterns))
            {
                ServiceResponse.Fail("User does not have claim to manage patterns!");
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

            return ServiceResponse.Ok();
        }
    }
}
