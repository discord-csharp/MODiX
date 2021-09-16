using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.RemoraShim.Errors;
using Modix.RemoraShim.Services;
using Modix.Services.Core;
using Modix.Services.Moderation;

using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Modix.RemoraShim.Responders
{
    internal class MuteRoleConfigurationResponder :
        INotificationHandler<Discord.ReadyNotification>, // We need to wait for the Discord.Net side to be ready, otherwise calling into Modix.Services methods can fail
        IResponder<IGuildCreate>,
        IResponder<IChannelCreate>,
        IResponder<IChannelUpdate>
    {
        public MuteRoleConfigurationResponder(
            IAuthorizationService authorizationService,
            IAuthorizationContextService authorizationContextService,
            IDiscordRestChannelAPI channelApi,
            IDesignatedChannelService designatedChannelService,
            IModerationService moderationService)
        {
            _authorizationService = authorizationService;
            _authorizationContextService = authorizationContextService;
            _channelApi = channelApi;
            _designatedChannelService = designatedChannelService;
            _moderationService = moderationService;
        }

        static MuteRoleConfigurationResponder()
            => _muteDeniedPermissionSet = new(_muteDeniedPermissions);

        public Task HandleNotificationAsync(Discord.ReadyNotification notification, CancellationToken cancellationToken)
        {
            _readySignal.Set();
            return Task.CompletedTask;
        }

        public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
        {
            _readySignal.Wait(ct);

            try
            {
                var authenticationResult = await _authorizationContextService.SetCurrentAuthenticatedUserAsync(gatewayEvent.ID);
                if (!authenticationResult.IsSuccess)
                    return authenticationResult;

                var muteRole = await _moderationService.GetOrCreateDesignatedMuteRoleAsync(gatewayEvent.ID.Value, _authorizationService.CurrentUserId!.Value);

                var unmoderatedChannels = await _designatedChannelService.GetDesignatedChannelIdsAsync(gatewayEvent.ID.Value, DesignatedChannelType.Unmoderated);

                var textChannels = gatewayEvent.Channels.Value
                    .Where(x => x.Type is ChannelType.GuildText or ChannelType.GuildVoice)
                    .Where(x => !unmoderatedChannels.Contains(x.ID.Value))
                    .ToImmutableArray();

                var errorChannels = new List<IChannel>();

                foreach (var channel in textChannels)
                {
                    var permissions = channel.PermissionOverwrites.Value
                        .SingleOrDefault(x => x.ID.Value == muteRole.Id);

                    if (permissions is null || _muteDeniedPermissions.Any(x => !permissions.Deny.HasPermission(x)))
                    {
                        var editChannelResult = await _channelApi.EditChannelPermissionsAsync(channel.ID, new Snowflake(muteRole.Id), deny: _muteDeniedPermissionSet,
                            type: PermissionOverwriteType.Role, reason: "Denying thread permissions for the mute role.", ct: ct);
                        if (!editChannelResult.IsSuccess)
                            errorChannels.Add(channel);
                    }
                }

                return errorChannels.Count > 0
                    ? Result.FromError(new ChannelConfigurationError("mute role configuration", errorChannels))
                    : Result.FromSuccess();
            }
            catch (Exception ex)
            {
                return Result.FromError(new ExceptionError(ex));
            }
        }

        public async Task<Result> RespondAsync(IChannelCreate gatewayEvent, CancellationToken ct = default)
            => await ConfigureChannelAsync(gatewayEvent, ct);

        public async Task<Result> RespondAsync(IChannelUpdate gatewayEvent, CancellationToken ct = default)
            => await ConfigureChannelAsync(gatewayEvent, ct);

        private async Task<Result> ConfigureChannelAsync(IChannel gatewayEvent, CancellationToken ct)
        {
            if (gatewayEvent.Type is not (ChannelType.GuildText or ChannelType.GuildVoice))
                return Result.FromSuccess();

            _readySignal.Wait(ct);

            try
            {
                var authenticationResult = await _authorizationContextService.SetCurrentAuthenticatedUserAsync(gatewayEvent.GuildID.Value);
                if (!authenticationResult.IsSuccess)
                    return authenticationResult;

                var muteRole = await _moderationService.GetOrCreateDesignatedMuteRoleAsync(gatewayEvent.GuildID.Value.Value, _authorizationService.CurrentUserId!.Value);

                var isUnmoderated = await _designatedChannelService.ChannelHasDesignationAsync(gatewayEvent.GuildID.Value.Value, gatewayEvent.ID.Value, DesignatedChannelType.Unmoderated, ct);

                if (isUnmoderated)
                    return Result.FromSuccess();

                var permissions = gatewayEvent.PermissionOverwrites.Value
                    .SingleOrDefault(x => x.ID.Value == muteRole.Id);

                if (permissions is null || _muteDeniedPermissions.Any(x => !permissions.Deny.HasPermission(x)))
                {
                    var editChannelResult = await _channelApi.EditChannelPermissionsAsync(gatewayEvent.ID, new Snowflake(muteRole.Id), deny: _muteDeniedPermissionSet,
                        type: PermissionOverwriteType.Role, reason: "Denying thread permissions for the mute role.", ct: ct);
                    if (!editChannelResult.IsSuccess)
                        return Result.FromError(new ChannelConfigurationError("mute role configuration", new[] { gatewayEvent }));
                }

                return Result.FromSuccess();
            }
            catch (Exception ex)
            {
                return Result.FromError(new ExceptionError(ex));
            }
        }

        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContextService _authorizationContextService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IModerationService _moderationService;

        private static readonly ManualResetEventSlim _readySignal = new();

        private static readonly DiscordPermission[] _muteDeniedPermissions =
        {
            DiscordPermission.AddReactions,
            DiscordPermission.SendMessages,
            DiscordPermission.Speak,
            DiscordPermission.RequestToSpeak,
            DiscordPermission.UsePublicThreads,
            DiscordPermission.UsePrivateThreads,
            (DiscordPermission)38, // TODO: update this when https://github.com/Nihlus/Remora.Discord/pull/91 is merged 
        };

        private static readonly DiscordPermissionSet _muteDeniedPermissionSet;
    }
}
