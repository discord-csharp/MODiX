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

        public PromotionDialogStartupBehavior(IMemoryCache memoryCache, IServiceProvider serviceProvider)
        {
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync()
        {
            using var serviceScope = _serviceProvider.CreateScope();
            var promotionDialogRepository = serviceScope.ServiceProvider.GetRequiredService<IPromotionDialogRepository>();
            var dialogs = await promotionDialogRepository.GetDialogsAsync();
            foreach (var dialog in dialogs)
            {
                if(!dialog.IsCampaignOpen)
                {
                    await promotionDialogRepository.TryDeleteAsync(dialog.MessageId);
                    break;
                }

                SetDialogCache(new CachedPromoDialog
                {
                    CampaignId = dialog.CampaignId,
                    MessageId = dialog.MessageId
                });
            }
        }

        private void SetDialogCache(CachedPromoDialog dialog)
        {
            _memoryCache.Set(GetKey(dialog.CampaignId), dialog);
            _memoryCache.Set(dialog.MessageId, dialog);
        }

        private static object GetKey(long id) => new { Target = "PromotionDialogBehavior", id };

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
