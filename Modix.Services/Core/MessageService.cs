using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    /// <summary>
    /// Describes a service that performs functions related to Discord messages.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Asynchronously finds a message in a guild where the channel is not known.
        /// </summary>
        /// <param name="guildId">The unique Discord snowflake ID of the guild in which the message belongs.</param>
        /// <param name="messageId">The unique Discord snowflake ID of the message to find.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes,
        /// containing the message, if found, or <see langword="null"/> if not.
        /// </returns>
        Task<IUserMessage> FindMessageAsync(ulong guildId, ulong messageId);
    }

    /// <inheritdoc />
    internal class MessageService : IMessageService
    {
        public MessageService(
            IDiscordSocketClient discordSocketClient,
            ISelfUserProvider selfUserProvider,
            IMessageRepository messageRepository)
        {
            _discordSocketClient = discordSocketClient;
            _selfUserProvider = selfUserProvider;
            _messageRepository = messageRepository;
        }

        /// <inheritdoc />
        public async Task<IUserMessage> FindMessageAsync(ulong guildId, ulong messageId)
        {
            var guild = await _discordSocketClient.GetGuildAsync(guildId);
            var guildMessage = await _messageRepository.GetMessage(messageId);

            if (guildMessage is { })
            {
                var channel = await guild.GetTextChannelAsync(guildMessage.ChannelId);
                return (await channel.GetMessageAsync(messageId)) as IUserMessage;
            }

            IMessage message = null;

            // We haven't found a message, now fetch all text
            // channels and attempt to find the message

            var channels = await guild.GetTextChannelsAsync();

            var selfUser = await _selfUserProvider.GetSelfUserAsync();
            var selfGuildUser = await guild.GetUserAsync(selfUser.Id);

            foreach (var channel in channels)
            {
                if (selfGuildUser.GetPermissions(channel).ReadMessageHistory)
                {
                    message = await channel.GetMessageAsync(messageId);

                    if (message is { })
                        break;
                }
            }

            return message as IUserMessage;
        }

        private readonly IDiscordSocketClient _discordSocketClient;
        private readonly ISelfUserProvider _selfUserProvider;
        private readonly IMessageRepository _messageRepository;
    }
}
