using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Data.Models;
using Serilog;

namespace Modix
{
    public class ModixBotHooks
    {

        public Task HandleLog(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Log.Error(message.ToString());
                    break;
                case LogSeverity.Debug:
                    Log.Debug(message.ToString());
                    break;
                case LogSeverity.Warning:
                    Log.Warning(message.ToString());
                    break;
                case LogSeverity.Error:
                    Log.Error(message.ToString());
                    break;
                case LogSeverity.Info:
                    Log.Information(message.ToString());
                    break;
                case LogSeverity.Verbose:
                    Log.Verbose(message.ToString());
                    break;
            }
            return Task.CompletedTask;
        }

        public async Task HandleMessage(SocketMessage messageParam)
        {
            var user = ((messageParam as SocketUserMessage)?.Author as SocketGuildUser);

            if (user == null) return;

            //var msg = new DiscordMessage()
            //{
                //AvatarId = user.AvatarId,
                //AvatarUrl = user.AvatarUrl,
                //Content = messageParam.Content,
                //CreatedAt = messageParam.Timestamp.DateTime,
                //Discriminator = user.Discriminator,
                //DiscriminatorValue = user.DiscriminatorValue,
                //Username = user.Username,
                //IsBot = messageParam.Author.IsBot,
                //MessageId = messageParam.Id,
                //Mention = messageParam.Author.Mention,
                //GuildId = user.Guild.Id,
                //Game = user.Game.ToString(),
                //Attachments = messageParam.Attachments.Select((attachment) => attachment.Url).ToArray(),
            //};

            //var res = new MessageRepository().InsertAsync(msg);
            //await res;
            //Logger.Info($"Logged message from {user.Username} in {user.Guild.Name}/{messageParam.Channel}");
        }
    }
}
