using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Modix.Common.Messaging;

namespace Modix.Services.Tags
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class TagCachePopulatingBehavior :
        INotificationHandler<JoinedGuildNotification>,
        INotificationHandler<GuildAvailableNotification>
    {
        private readonly ITagService _tagService;

        public TagCachePopulatingBehavior(ITagService tagService)
        {
            _tagService = tagService;
        }

        public async Task HandleNotificationAsync(JoinedGuildNotification notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (notification.Guild is not IGuild { Available: true } guild)
                return;

            await _tagService.RefreshCache(guild.Id);
        }

        public async Task HandleNotificationAsync(GuildAvailableNotification notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _tagService.RefreshCache(notification.Guild.Id);
        }
    }
}
