using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.Mentions
{
    public class MentionCommand : IRequest<bool>
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

    public class MentionCommandHandler : IRequestHandler<MentionCommand, bool>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDesignatedRoleService _designatedRoleService;

        public MentionCommandHandler(IAuthorizationService authorizationService,
            IDesignatedRoleService designatedRoleService)
        {
            _authorizationService = authorizationService;
            _designatedRoleService = designatedRoleService;
        }

        public async Task<bool> Handle(MentionCommand request, CancellationToken cancellationToken)
        {
            if (request.Role is null)
                throw new ArgumentNullException(nameof(request.Role));

            if (request.Channel is null)
                throw new ArgumentNullException(nameof(request.Channel));

            var message = request.Message ?? string.Empty;

            if (request.Role.IsMentionable)
            {
                await SendMessage();
                return true;
            }

            if (!_authorizationService.HasClaim(AuthorizationClaim.MentionRestrictedRole))
            {
                await request.Channel.SendMessageAsync("You cannot mention roles!");
                return false;
            }

            if (!await _designatedRoleService.RoleHasDesignationAsync(request.Role.Guild.Id,
                request.Role.Id, DesignatedRoleType.RestrictedMentionability))
            {
                await request.Channel.SendMessageAsync("You cannot mention this role!");
                return false;
            }

            try
            {
                await request.Role.ModifyAsync(x => x.Mentionable = true);
                await SendMessage();
            }
            finally
            {
                await request.Role.ModifyAsync(x => x.Mentionable = false);
            }

            return true;

            Task SendMessage() =>
                request.Channel.SendMessageAsync($"{request.Role.Mention} {message}");
        }
    }
}
