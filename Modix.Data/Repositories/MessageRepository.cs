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
        Task<IReadOnlyCollection<MessageEntity>> GetRecentUserMessagesAsync(ulong guildId, ulong userId, TimeSpan timespan);

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

        public async Task<IReadOnlyCollection<MessageEntity>> GetRecentUserMessagesAsync(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var utcNow = DateTimeOffset.UtcNow;
            var maxTimespan = utcNow - DateTimeOffset.MinValue;
            var timeRange = timespan > maxTimespan ? maxTimespan : timespan;
            var earliestDateTime = utcNow - timeRange;

            var messages = await ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.AuthorId == userId && x.Timestamp >= earliestDateTime)
                .ToArrayAsync();

            return Array.AsReadOnly(messages);
        }
    }
}
