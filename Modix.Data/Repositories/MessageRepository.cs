using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    public interface IMessageRepository
    {
        Task<int> GetUserMessageCountAsync(ulong guildId, ulong userId);

        Task<int> GetUserMessageCountAsync(ulong guildId, ulong userId, TimeSpan timespan);

        Task AddAsync(MessageEntity message);

        Task RemoveAsync(ulong messageId);
    }

    public class MessageRepository : RepositoryBase, IMessageRepository
    {
        public MessageRepository(ModixContext context)
            : base(context) { }

        public async Task AddAsync(MessageEntity message)
        {
            await ModixContext.Messages.AddAsync(message);
            await ModixContext.SaveChangesAsync();
        }

        public async Task RemoveAsync(ulong messageId)
        {
            if (await ModixContext.Messages.FindAsync(messageId) is MessageEntity message)
            {
                ModixContext.Messages.Remove(message);
                await ModixContext.SaveChangesAsync();
            }
        }

        public Task<int> GetUserMessageCountAsync(ulong guildId, ulong userId) =>
            GetUserMessageCountAsync(guildId, userId, TimeSpan.MaxValue);

        public async Task<int> GetUserMessageCountAsync(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var utcNow = DateTimeOffset.UtcNow;
            var maxTimespan = utcNow - DateTimeOffset.MinValue;
            var timeRange = timespan > maxTimespan ? maxTimespan : timespan;
            var earliestDateTime = utcNow - timeRange;

            return await ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.AuthorId == userId && x.Timestamp >= earliestDateTime)
                .CountAsync();
        }
    }
}
