using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Services.Stocks
{
    public class StockOverview
    {
        public string Information { get; set; }

        public string Symbol { get; set; }

        public IEnumerable<StockResult> StockResults { get; set; }
    }
}
