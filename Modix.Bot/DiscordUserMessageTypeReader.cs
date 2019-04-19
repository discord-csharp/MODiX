using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Data.Repositories;
using Modix.Services.Core;

namespace Modix
{
    public class DiscordUserMessageTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (ulong.TryParse(input, out var messageId))
            {
                var value = await FindMessageInUnknownChannelAsync(context, messageId, services);
                var userMessage = value as IUserMessage;

                if (userMessage is null)
                    return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found.");

                return TypeReaderResult.FromSuccess(new DiscordUserMessage(userMessage));
            }

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found.");
        }

        // TODO: extract to service layer after the PR that adds the DB check to the quote service version of this is merged.
        private async Task<IMessage> FindMessageInUnknownChannelAsync(ICommandContext context, ulong messageId, IServiceProvider services)
        {
            var messageRepository = (IMessageRepository)services.GetService(typeof(IMessageRepository));
            var guildMessage = await messageRepository.GetMessage(messageId);

            if (guildMessage is { })
            {
                var channel = await context.Guild.GetTextChannelAsync(guildMessage.ChannelId);
                return await channel.GetMessageAsync(messageId);
            }

            IMessage message = null;

            // We haven't found a message, now fetch all text
            // channels and attempt to find the message

            var channels = await context.Guild.GetTextChannelsAsync();

            var selfUserProvider = (ISelfUserProvider)services.GetService(typeof(ISelfUserProvider));
            var selfUser = await selfUserProvider.GetSelfUserAsync();
            var selfGuildUser = await context.Guild.GetUserAsync(selfUser.Id);

            foreach (var channel in channels)
            {
                if (selfGuildUser.GetPermissions(channel).ReadMessageHistory)
                {
                    message = await channel.GetMessageAsync(messageId);

                    if (message is { })
                        break;
                }
            }

            return message;
        }
    }

    public class DiscordUserMessage
    {
        public DiscordUserMessage(IUserMessage userMessage)
        {
            _userMessage = userMessage;
        }

        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => _userMessage.Reactions;

        public MessageType Type => _userMessage.Type;

        public MessageSource Source => _userMessage.Source;

        public bool IsTTS => _userMessage.IsTTS;

        public bool IsPinned => _userMessage.IsPinned;

        public string Content => _userMessage.Content;

        public DateTimeOffset Timestamp => _userMessage.Timestamp;

        public DateTimeOffset? EditedTimestamp => _userMessage.EditedTimestamp;

        public IMessageChannel Channel => _userMessage.Channel;

        public IUser Author => _userMessage.Author;

        public IReadOnlyCollection<IAttachment> Attachments => _userMessage.Attachments;

        public IReadOnlyCollection<IEmbed> Embeds => _userMessage.Embeds;

        public IReadOnlyCollection<ITag> Tags => _userMessage.Tags;

        public IReadOnlyCollection<ulong> MentionedChannelIds => _userMessage.MentionedChannelIds;

        public IReadOnlyCollection<ulong> MentionedRoleIds => _userMessage.MentionedRoleIds;

        public IReadOnlyCollection<ulong> MentionedUserIds => _userMessage.MentionedUserIds;

        public MessageActivity Activity => _userMessage.Activity;

        public MessageApplication Application => _userMessage.Application;

        public DateTimeOffset CreatedAt => _userMessage.CreatedAt;

        public ulong Id => _userMessage.Id;

        public Task AddReactionAsync(IEmote emote, RequestOptions options = null) => _userMessage.AddReactionAsync(emote, options);

        public Task DeleteAsync(RequestOptions options = null) => _userMessage.DeleteAsync(options);

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null) => _userMessage.GetReactionUsersAsync(emoji, limit, options);

        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null) => _userMessage.ModifyAsync(func, options);

        public Task PinAsync(RequestOptions options = null) => _userMessage.PinAsync(options);

        public Task RemoveAllReactionsAsync(RequestOptions options = null) => _userMessage.RemoveAllReactionsAsync(options);

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null) => _userMessage.RemoveReactionAsync(emote, user, options);

        public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name) => _userMessage.Resolve(userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);

        public Task UnpinAsync(RequestOptions options = null) => _userMessage.UnpinAsync(options);

        private readonly IUserMessage _userMessage;
    }
}
