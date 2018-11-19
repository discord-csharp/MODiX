using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<MessageEntity>> GetUserMessages(ulong guildId, ulong userId);

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

        public Task<IEnumerable<MessageEntity>> GetUserMessages(ulong guildId, ulong userId)
        {
            throw new System.NotImplementedException();
        }
    }
}
