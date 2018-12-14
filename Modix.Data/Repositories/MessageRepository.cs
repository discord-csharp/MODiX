using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    public interface IMessageRepository
    {
        Task<IReadOnlyDictionary<DateTime, int>> GetGuildUserMessageCountByDate(ulong guildId, ulong userId, TimeSpan timespan);

        Task<IReadOnlyDictionary<ulong, int>> GetGuildUserMessageCountByChannel(ulong guildId, ulong userId, TimeSpan timespan);

        Task<IReadOnlyDictionary<GuildUserEntity, int>> GetPerUserMessageCounts(ulong guildId, TimeSpan timespan, int userCount = 10);

        Task CreateAsync(MessageEntity message);

        Task DeleteAsync(ulong messageId);
    }

    public class MessageRepository : RepositoryBase, IMessageRepository
    {
        public MessageRepository(ModixContext context)
            : base(context) { }

        public async Task CreateAsync(MessageEntity message)
        {
            await ModixContext.Messages.AddAsync(message);
            await ModixContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(ulong messageId)
        {
            if (await ModixContext.Messages.FindAsync(messageId) is MessageEntity message)
            {
                ModixContext.Messages.Remove(message);
                await ModixContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetGuildUserMessageCount(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            return await ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.AuthorId == userId && x.Timestamp >= earliestDateTime)
                .CountAsync();
        }

        public async Task<IReadOnlyDictionary<DateTime, int>> GetGuildUserMessageCountByDate(ulong guildId, ulong userId, TimeSpan timespan)
        {
            return await GetGuildUserMessages(guildId, userId, timespan)
                .GroupBy(x => x.Timestamp.Date)
                .ToDictionaryAsync(x => x.Key, x => x.Count());
        }

        public async Task<IReadOnlyDictionary<ulong, int>> GetGuildUserMessageCountByChannel(ulong guildId, ulong userId, TimeSpan timespan)
        {
            return await GetGuildUserMessages(guildId, userId, timespan)
                .GroupBy(x => x.ChannelId)
                .ToDictionaryAsync(x => x.Key, x => x.Count());
        }

        public async Task<IReadOnlyDictionary<GuildUserEntity, int>> GetPerUserMessageCounts(ulong guildId, TimeSpan timespan, int userCount = 10)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            var result = await ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.Timestamp >= earliestDateTime)
                .GroupBy(x => x.AuthorId)
                .OrderByDescending(x => x.Count())
                .Take(userCount)
                .ToListAsync();

            var userIds = result.Select(d => d.Key).ToArray();

            var userQuery = await ModixContext.GuildUsers
                .Where(x => x.GuildId == guildId)
                .Where(x => userIds.Contains(x.UserId))
                .Include(x => x.User)
                .AsNoTracking()
                .ToDictionaryAsync(x => x.UserId, x => x);

            return result.ToDictionary(x => userQuery[x.Key], x => x.Count());
        }

        private IQueryable<MessageEntity> GetGuildUserMessages(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            return ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.AuthorId == userId && x.Timestamp >= earliestDateTime);
        }
    }
}
