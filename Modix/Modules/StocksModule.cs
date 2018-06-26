using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using Modix.Data.Models;
using Modix.Services.Stocks;
using System.Text;
using System;

namespace Modix.Modules
{
    [Name("Stocks"), Summary("Gets market details.")]
    public class StocksModule : ModuleBase
    {
        private readonly ModixConfig _config;

        public StocksModule(ModixConfig config)
        {
            _config = config;
        }

        [Command("stock"), Summary("Gets the latest trade information for a given symbol.")]
        public async Task Run(string symbol)
        {
            var response = await new AlphaVantageStocksService().GetStockInformation(_config.AlphaVantageToken, "TIME_SERIES_INTRADAY", symbol);
            if (response == null)
            {
                await ReplyAsync($"Unable to obtain market details for symbol [{symbol}]");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine($"Trade information for: [{response.Symbol}]");

            var results = response.StockResults.Where(r => r.Date == DateTime.Today);
            bool today = true;
            if (!results.Any())
            {
                today = false;
                results = response.StockResults.Where(r => r.Date >= DateTime.Today.AddDays(-1) && r.Date < DateTime.Today);
            }

            decimal dailyHigh = 0;
            decimal dailyLow = 0;
            decimal close = results.First().Close;
            decimal open = results.Last().Open;
            foreach (var result in results)
            {
                if (dailyHigh < result.High)
                {
                    dailyHigh = result.High;
                }

                if (dailyLow > result.Low || dailyLow == 0)
                {
                    dailyLow = result.Low;
                }

                sb.AppendLine($"{result.Date.ToString("hh:mm tt")}\tOpen: ${result.Open}\tClose: ${result.Close}\tHigh: ${result.High}\tLow: ${result.Low}\tVolume: {result.Volume}");
            }

            sb.AppendLine($"\nSummary for [{symbol}]");
            sb.AppendLine("---------------");
            sb.AppendLine($"- opened at: ${open}");
            sb.AppendLine($"- closed at: ${close}");
            sb.AppendLine($"- growth: ${close - open}");
            sb.AppendLine($"- highest: ${dailyHigh}");
            sb.AppendLine($"- lowest: ${dailyLow}");
            sb.AppendLine("---------------");

            if (!today)
            {
                sb.AppendLine("\nMarket hasn't opened yet today - showing yesterday's results");
            }
            sb.AppendLine("```");

            await ReplyAsync(sb.ToString());

        }
    }
}
