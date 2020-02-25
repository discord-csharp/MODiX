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
using Modix.Data.Repositories;
using Npgsql;

namespace Modix.Bot.Behaviors
{
    public class PromotionDialogStartupBehavior :IHostedService
    {

        private IMemoryCache _memoryCache { get; }

        private IServiceProvider _serviceProvider { get; }

        private IPromotionDialogRepository _promotionDialogRepository { get; set; }

        public PromotionDialogStartupBehavior(IMemoryCache memoryCache,
            IServiceProvider serviceProvider
            )
        {
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var serviceScope = _serviceProvider.CreateScope();
                _promotionDialogRepository = serviceScope.ServiceProvider.GetRequiredService<IPromotionDialogRepository>();
                var dialogs = await _promotionDialogRepository.GetDialogsAsync();
                foreach (var dialog in dialogs)
                {
                    SetDialogCache(dialog.CampaignId, dialog.MessageId, new CachedPromoDialog
                    {
                        CampaignId = dialog.CampaignId,
                        MessageId = dialog.MessageId
                    });
                }
            }
            catch(PostgresException e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetDialogCache(long campaign, ulong message, CachedPromoDialog dialog)
        {
            _memoryCache.Set(campaign, dialog);
            _memoryCache.Set(message, dialog);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;

        }

    }
}
