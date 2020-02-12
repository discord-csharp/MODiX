using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Repositories;
using Modix.Services;

namespace Modix.Bot.Behaviors
{
    public class PromotionDialogStartupBehavior :IBehavior
    {

        private IMemoryCache _memoryCache { get; }

        private IServiceProvider _serviceProvider { get; }

        private IPromotionDialogRepository _promotionDialogRepository { get; set; }

        public PromotionDialogStartupBehavior(IMemoryCache memoryCache,
            IServiceProvider serviceProvider)
        {
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync()
        {
            using var serviceScope = _serviceProvider.CreateScope();
            _promotionDialogRepository = serviceScope.ServiceProvider.GetRequiredService<IPromotionDialogRepository>();
            var dialogs = await _promotionDialogRepository.GetDialogsAsync();
            foreach (var dialog in dialogs)
            {
                if (dialog.IsCampaignOpen == false)
                {
                    await _promotionDialogRepository.TryDeleteAsync(dialog.MessageId);
                    break;
                }

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

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
