using YahooFinanceApi;
using ELT3.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Serilog.Extensions.Logging;

namespace ELT3.Services
{
    public class YahooApiClient
    {
        private readonly ILogger<YahooApiClient> _logger;

        private readonly HttpClient _httpClient;

        public YahooApiClient(ILogger<YahooApiClient> logger, HttpClient httpClient )
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<StockQuote>> GetStockQuotesAsync(IEnumerable<string> symbols)
        {
            var symbolsArray = symbols.ToArray();
            _logger.LogInformation("Request a quote from Yahoo Finance for: {Symbols}", string.Join(", ", symbolsArray));

            try
            {
                var securities = await Yahoo.Symbols(symbolsArray)
                    .Fields(
                        Field.Symbol,
                        Field.RegularMarketPrice,
                        Field.RegularMarketChangePercent,
                        Field.RegularMarketTime
                    )
                    .QueryAsync();

                if (securities == null || !securities.Any())
                {
                    _logger.LogWarning("Yahoo Finance did not return any data.");
                    return Enumerable.Empty<StockQuote>();
                }

                // Using LINQ for cleaner list creation
                var quotes = securities.Values
                    .Where(s => s.RegularMarketPrice > 0)
                    .Select(s => new StockQuote
                    {
                        Symbol = s.Symbol,
                        Price = (decimal)s.RegularMarketPrice,
                        ChangesPercentage = (decimal)s.RegularMarketChangePercent,
                        LastUpdateTime = DateTimeOffset.FromUnixTimeSeconds(s.RegularMarketTime).UtcDateTime
                    })
                    .ToList();

                _logger.LogInformation("received {Count} quotations.", quotes.Count);
                return quotes;
            }
            catch (Exception ex)
            {
                // If Yahoo blocks the request, we will see it in the logs.
                _logger.LogError(ex, "Error accessing Yahoo Finance API.");
                return Enumerable.Empty<StockQuote>();
            }
        }
    
    
    }
}