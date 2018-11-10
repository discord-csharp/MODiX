using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Modix.Data.Models.Promotions;
using Modix.Data.ExpandableQueries;

namespace Modix.Data.Test
{
    [TestFixture]
    public class Sandbox
    {
        [Test]
        public async Task Test()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ModixContext>(options =>
            {
                options.UseNpgsql("User ID=X;Password=X;Host=localhost;Port=5432;Database=ModixTest;");
            });

            var serviceProvider = services.BuildServiceProvider();

            var modixContext = serviceProvider.GetRequiredService<ModixContext>();

            var campaign = await modixContext
                .PromotionCampaigns
                .AsNoTracking()
                //.AsProjectable()
                .AsExpandable()
                //.Select(PromotionCampaignBrief.FromEntityProjection)
                .Select(PromotionCampaignDetails.FromEntityProjection)
                .Where(x => x.Id == 9)
                .FirstAsync();

            Console.WriteLine(JsonConvert.SerializeObject(campaign, Formatting.Indented));
        }
    }
}
