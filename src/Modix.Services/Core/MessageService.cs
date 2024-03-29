using System.Linq;
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
        Task<IMessage> FindMessageAsync(ulong guildId, ulong messageId);
    }

    /// <inheritdoc />
    internal class MessageService : IMessageService
    {
        public MessageService(
            DiscordSocketClient discordSocketClient,
            IMessageRepository messageRepository)
        {
            _discordSocketClient = discordSocketClient;
            _messageRepository = messageRepository;
        }

        /// <inheritdoc />
        public async Task<IMessage> FindMessageAsync(ulong guildId, ulong messageId)
        {
            var guild = _discordSocketClient.GetGuild(guildId);
            var trackedMessage = await _messageRepository.GetMessage(messageId);

            var selfGuildUser = guild.GetUser(_discordSocketClient.CurrentUser.Id);

            var channels = guild.TextChannels
                .AsEnumerable();

            // If we've tracked the message, lookup the channel, and check that one first.
            if (trackedMessage is { })
                channels = channels
                    .Where(x => x.Id != trackedMessage.ChannelId)
                    .Prepend(guild.GetTextChannel(trackedMessage.ChannelId));

            // Search through all available channels to find the message.
            foreach (var channel in channels)
            {
                // Only try and download the message if we have permissions to
                var message = selfGuildUser.GetPermissions(channel).ReadMessageHistory
                    ? await channel.GetMessageAsync(messageId)
                    : channel.GetCachedMessage(messageId);

                if (message is { })
                    return message;
            }

            // If we made it here, we couldn't find the message.
            return null;
        }

        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IMessageRepository _messageRepository;
    }
}
