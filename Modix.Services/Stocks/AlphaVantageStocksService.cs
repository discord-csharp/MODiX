using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Modix.Services.Stocks
{
    public class AlphaVantageStocksService
    {
        private string _timeSeriesApiReferenceUrl = "https://www.alphavantage.co/query?function={0}&symbol={1}&interval=60min&apikey={2}";

        public async Task<StockOverview> GetStockInformation(string token, string function, string symbol)
        {
            _timeSeriesApiReferenceUrl = string.Format(_timeSeriesApiReferenceUrl, function, symbol, token);

            var client = new HttpClient();
            var response = await client.GetAsync(_timeSeriesApiReferenceUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while querying the stocks service.");
            }

            // The API is awful - need to dig pretty deep and form our own model based on the response.
            var jsonResponse = await response.Content.ReadAsStringAsync();
            if(jsonResponse.Contains("Error Message"))
            {
                return null;
            }

            var seriesRoot = JsonConvert.DeserializeObject<AlphaVantageStocksResponse>(jsonResponse);
            var seriesRaw = seriesRoot.SeriesData.Values.First().ToString();

            // This is a much cleaner object to work with when constructing this API's result set.
            var seriesParsed = JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, string>>>(seriesRaw);

            // Finally, a clean model.
            return new StockOverview
            {
                Information = seriesRoot.Metadata.Information,
                Symbol = seriesRoot.Metadata.Symbol,
                StockResults = seriesParsed.Select(result => new StockResult
                {
                    Date = DateTime.Parse(result.Key),
                    Open = decimal.Round(decimal.Parse(result.Value[result.Value.Keys.Single(k => k.Contains("open"))]), 2, MidpointRounding.AwayFromZero),
                    High = decimal.Round(decimal.Parse(result.Value[result.Value.Keys.Single(k => k.Contains("high"))]), 2, MidpointRounding.AwayFromZero),
                    Low = decimal.Round(decimal.Parse(result.Value[result.Value.Keys.Single(k => k.Contains("low"))]), 2, MidpointRounding.AwayFromZero),
                    Close = decimal.Round(decimal.Parse(result.Value[result.Value.Keys.Single(k => k.Contains("close"))]), 2, MidpointRounding.AwayFromZero),
                    Volume = int.Parse(result.Value[result.Value.Keys.Single(k => k.Contains("volume"))]),
                })
            };
        }
    }
}
