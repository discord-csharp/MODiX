using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Modix.Data.Models.Core;
using Modix.RemoraShim.Models;
using Modix.RemoraShim.Services;
using Modix.Services.Core;
using Modix.Services.Tags;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Modix.RemoraShim.Responders
{
    public class InlineTagResponder : IResponder<IMessageCreate>
    {
        private static readonly Regex _inlineTagRegex = new(@"\$(\S+)\b");
        private readonly IAuthorizationService _authService;
        private readonly ITagService _tagService;
        private readonly IThreadService _threadService;
        private readonly IDiscordRestChannelAPI _channelApi;

        public InlineTagResponder(IAuthorizationService authService, ITagService tagService, IThreadService threadService, IDiscordRestChannelAPI channelApi)
        {
            _authService = authService;
            _tagService = tagService;
            _threadService = threadService;
            _channelApi = channelApi;
        }

        public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
        {
            if (gatewayEvent.Author.IsBot.HasValue && gatewayEvent.Author.IsBot.Value)
            {
                return Result.FromSuccess();
            }

            if (gatewayEvent.Author.IsSystem.HasValue && gatewayEvent.Author.IsSystem.Value)
            {
                return Result.FromSuccess();
            }

            if(! await _threadService.IsThreadChannelAsync(gatewayEvent.ChannelID, ct))
            {
                return Result.FromSuccess();
            }

            //TODO: Refactor when we have a configurable prefix
            if (gatewayEvent.Content.StartsWith('!'))
            { return Result.FromSuccess(); }

            //Remove code blocks from the message we are processing
            var content = Regex.Replace(gatewayEvent.Content, @"(`{1,3}).*?(.\1)", string.Empty, RegexOptions.Singleline);
            //Remove quotes from the message we are processing
            content = Regex.Replace(content, "^>.*$", string.Empty, RegexOptions.Multiline);

            if (string.IsNullOrWhiteSpace(content))
            { return Result.FromSuccess(); }

            var match = _inlineTagRegex.Match(content);
            if (!match.Success)
            { return Result.FromSuccess(); }

            var tagName = match.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(tagName))
            { return Result.FromSuccess(); }

            var roles = new List<ulong>();

            if (gatewayEvent.Member.HasValue && gatewayEvent.Member.Value.Roles.HasValue)
            {
                roles = gatewayEvent.Member.Value.Roles.Value.Select(a => a.Value).ToList();
            }
            roles.Add(gatewayEvent.GuildID.Value.Value);

            if (await _authService.HasClaimsAsync(gatewayEvent.Author.ID.Value, gatewayEvent.GuildID.Value.Value, roles, AuthorizationClaim.UseTag) == false)
            { return Result.FromSuccess(); }
            if (await _tagService.TagExistsAsync(gatewayEvent.GuildID.Value.Value, tagName) == false)
            { return Result.FromSuccess(); }

            try
            {
                await _authService.OnAuthenticatedAsync(gatewayEvent.Author.ID.Value, gatewayEvent.GuildID.Value.Value, roles);
                var tag = await _tagService.GetTagAsync(gatewayEvent.GuildID.Value.Value, tagName);

                await _channelApi.CreateMessageAsync(gatewayEvent.ChannelID, new Optional<string>(tag.Content), allowedMentions: new NoAllowedMentions(), ct: ct);
            }
            catch (InvalidOperationException ex)
            {
                var embed = new Embed(
                    Title: "Error invoking inline tag",
                    Colour: Color.Red,
                    Description: ex.Message);

                await _channelApi.CreateMessageAsync(gatewayEvent.ChannelID, embeds: new[] { embed }, allowedMentions: new NoAllowedMentions(), ct: ct);
            }
            return Result.FromSuccess();
        }
    }
}
