using YahooFinanceApi;
using ELT3.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ELT3.Services
{
    public class YahooApiClient
    {
        public async Task<IEnumerable<StockQuote>> GetStockQuotesAsync(IEnumerable<string> symbols)
        {
            var securities = await Yahoo.Symbols(symbols.ToArray())
                .Fields(Field.Symbol,
                    Field.RegularMarketPrice,
                    Field.RegularMarketChangePercent,
                    Field.RegularMarketTime
                    )
                .QueryAsync();
            
            if (!securities.Any())
            {
                return Enumerable.Empty<StockQuote>();
            }

            var quotes = new List<StockQuote>();

            foreach (var security in securities.Values)
            {
                // Check if price > 0 (if no data, usually 0.0 or NaN is returned)
                if (security.RegularMarketPrice > 0.0) 
                {
                    DateTime utcTime = DateTimeOffset.FromUnixTimeSeconds(security.RegularMarketTime).UtcDateTime;  
                    
                    DateTime lastUpdateUtc = DateTimeOffset.FromUnixTimeSeconds(security.RegularMarketTime).UtcDateTime;                    
                    quotes.Add(new StockQuote
                    {
                        Symbol = security.Symbol,
                        // Direct conversion of double to decimal
                        Price = (decimal)security.RegularMarketPrice, 
                        ChangesPercentage = (decimal)security.RegularMarketChangePercent,
                        LastUpdateTime = lastUpdateUtc,                        
                    });
                }
            }

            return quotes;
        }
    }
}