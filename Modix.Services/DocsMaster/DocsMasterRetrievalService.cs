using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Modix.Services.DocsMaster.Models;
using Newtonsoft.Json;

namespace Modix.Services.DocsMaster
{
    public class DocsMasterRetrievalService
    {
        private readonly HttpClient _client;

        public DocsMasterRetrievalService(HttpClient client)
        {
            _client = client;
        }

        public async Task<ManualEntryModel> GetManualEntryFromLatest(string manualName, string entryName)
        {
            var formattableString = $"http://docsmaster.net/ManualEntry/{manualName}/{entryName}";
            var result = await _client.GetAsync(formattableString);
            var str = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ManualEntryModel>(str);
        }

        public async Task<ManualEntryModel> GetManualEntryFromVersion(string manualName, string entryName, string version)
        {
            var result = await _client.GetAsync($"http://docsmaster.net/ManualEntry/{manualName}/{entryName}/{version}");
            return JsonConvert.DeserializeObject<ManualEntryModel>(await result.Content.ReadAsStringAsync());
        }

        public async Task<IEnumerable<string>> GetAllVersionsAsync(string manualName)
        {
            var result = await _client.GetAsync($"http://docsmaster.net/ManualEntry/{manualName}");
            return JsonConvert.DeserializeObject<List<string>>(await result.Content.ReadAsStringAsync());
        }
    }
}