using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.Mentions
{
    public class MentionCommand : INotification
    {
        public MentionCommand(IRole role, IMessageChannel channel, string message)
        {
            Role = role;
            Channel = channel;
            Message = message;
        }

        public IRole Role { get; }
        public IMessageChannel Channel { get; }
        public string Message { get; }
    }

    public class MentionCommandHandler : INotificationHandler<MentionCommand>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDesignatedRoleService _designatedRoleService;

        public MentionCommandHandler(IAuthorizationService authorizationService,
            IDesignatedRoleService designatedRoleService)
        {
            _authorizationService = authorizationService;
            _designatedRoleService = designatedRoleService;
        }

        public async Task Handle(MentionCommand notification,
            CancellationToken cancellationToken)
        {
            if (notification.Role is null)
                throw new ArgumentNullException(nameof(notification.Role));

            if (notification.Channel is null)
                throw new ArgumentNullException(nameof(notification.Channel));

            var message = notification.Message ?? string.Empty;

            if (notification.Role.IsMentionable)
            {
                await SendMessage();
                return;
            }

            if (!_authorizationService.HasClaim(AuthorizationClaim.MentionRestrictedRole))
            {
                await notification.Channel.SendMessageAsync("You cannot mention roles!");
                return;
            }

            if (!await _designatedRoleService.RoleHasDesignationAsync(notification.Role.Guild.Id,
                    notification.Role.Id, DesignatedRoleType.RestrictedMentionability))
            {
                await notification.Channel.SendMessageAsync("You cannot this role!");
                return;
            }

            try
            {
                await notification.Role.ModifyAsync(x => x.Mentionable = true);
                await SendMessage();
            }
            finally
            {
                await notification.Role.ModifyAsync(x => x.Mentionable = false);
            }

            Task SendMessage() =>
                notification.Channel.SendMessageAsync($"{notification.Role.Mention} {message}");
        }
    }
}
