using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Modix.Services.Promotions
{
    public class FilePromotionRepository : IPromotionRepository
    {
        //TODO: Un-hardcode this
        private const string _filePath = @"c:\app\config\promotions.json";

        private List<PromotionCampaign> _campaigns = null;
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };

        public async Task AddCommentToCampaign(PromotionCampaign campaign, PromotionComment comment)
        {
            comment.Id = (campaign.Comments.Any() ? campaign.Comments.Max(d=>d.Id) + 1 : 0);

            campaign.Comments.Add(comment);
            await Save();
        }

        private async Task EnsureLoaded()
        {
            if (_campaigns == null)
            {
                await Load();
            }
        }

        public async Task<PromotionCampaign> GetCampaign(int id)
        {
            await EnsureLoaded();
            return _campaigns.First(d => d.Id == id);
        }

        public async Task<IEnumerable<PromotionCampaign>> GetCampaigns()
        {
            await EnsureLoaded();
            return _campaigns.AsEnumerable();
        }

        public async Task AddCampaign(PromotionCampaign campaign)
        {
            campaign.Id = (_campaigns.Any() ? _campaigns.Max(d => d.Id) + 1 : 0);

            _campaigns.Add(campaign);
            await Save();
        }

        public async Task UpdateCampaign(PromotionCampaign campaign)
        {
            await Save();
        }

        private async Task Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

            using (FileStream sourceStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(sourceStream))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(_campaigns, _settings));
            }
        }

        private async Task Load()
        {
            if (!File.Exists(_filePath))
            {
                await Save();
            }

            using (FileStream readStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(readStream))
            {
                string content = await reader.ReadToEndAsync();

                _campaigns = JsonConvert.DeserializeObject<List<PromotionCampaign>>(content, _settings);
            }

            if (_campaigns == null)
            {
                _campaigns = new List<PromotionCampaign>();
            }
        }
    }
}
