using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Modix.Services.DocsMaster.Models;
using Newtonsoft.Json;

namespace Modix.Services.DocsMaster
{
    public class DocsMasterRetrievalService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DocsMasterRetrievalService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ManualEntryModel> GetManualEntryFromLatestAsync(string manualName, string entryName)
        {
            var formattableString = $"http://docsmaster.net/ManualEntry/{manualName}/{entryName}";
            var result = await _httpClientFactory.CreateClient().GetAsync(formattableString);
            var str = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ManualEntryModel>(str);
        }

        public async Task<ManualEntryModel> GetManualEntryFromVersionAsync(string manualName, string entryName, string version)
        {
            var result = await _httpClientFactory.CreateClient().GetAsync($"http://docsmaster.net/ManualEntry/{manualName}/{entryName}/{version}");
            return JsonConvert.DeserializeObject<ManualEntryModel>(await result.Content.ReadAsStringAsync());
        }

        public async Task<IEnumerable<string>> GetAllVersionsAsync(string manualName)
        {
            var result = await _httpClientFactory.CreateClient().GetAsync($"http://docsmaster.net/ManualEntry/{manualName}");
            return JsonConvert.DeserializeObject<List<string>>(await result.Content.ReadAsStringAsync());
        }
    }
}
