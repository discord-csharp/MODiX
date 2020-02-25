using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modix.Data;
using Modix.Data.Models.Promotions;

namespace Modix.Bot.Behaviors
{
    public class PromotionDialogStartupBehavior :IHostedService
    {
        private IServiceProvider _serviceProvider { get; }

        private IMemoryCache _memoryCache { get; }

        public PromotionDialogStartupBehavior(IServiceProvider serviceProvider, IMemoryCache memoryCache)
        {
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var serviceScope = _serviceProvider.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<ModixContext>();
            var dialogs = await context.Set<PromotionDialogEntity>().AsNoTracking()
                .Where(x => x.Campaign.Outcome != null)
                .ToListAsync();

            foreach (var dialog in dialogs)
            {
                SetDialogCache(dialog.CampaignId, dialog.MessageId, new CachedPromoDialog
                {
                    CampaignId = dialog.CampaignId,
                    MessageId = dialog.MessageId
                });
            }
        }

        private void SetDialogCache(long campaign, ulong message, CachedPromoDialog dialog)
        {
            _memoryCache.Set(campaign, dialog);
            _memoryCache.Set(message, dialog);
        }

        public Task StopAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    }
}
