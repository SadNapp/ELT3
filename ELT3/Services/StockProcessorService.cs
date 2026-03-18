using ELT3.Data;
using ELT3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELT3.Services
{
    public class StockProcessorService
    {
        private readonly YahooApiClient _yahooApiClient;
        private readonly ILogger<StockProcessorService> _logger;
        private readonly AppDbContext _dbContext;

        public StockProcessorService
            (
            YahooApiClient yahooApiClient,
                ILogger<StockProcessorService> logger,
                AppDbContext dbContext
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _yahooApiClient = yahooApiClient;
        }

        public async Task ProcessAsync()
        {
            var symbols = new List<string> { "AAPL", "MSFT", "GOOGL", "TSLA", "AMZN", "NVDA", "META" }; ;

            _logger.LogInformation("Starting stock processing for symbols: {Symbols}", string.Join(", ", symbols));

            try
            {
                var quotes = await _yahooApiClient.GetStockQuotesAsync(symbols);

                if (!quotes.Any())
                {
                    _logger.LogWarning("No data received from API. Skipping loop");
                     return;
                }
                var now = DateTime.UtcNow;
                foreach (var quote in quotes)
                {
                    quote.RecordedAt = now;
                    _dbContext.Quotes.Add(quote);

                    _logger.LogInformation("processed: {Symbol} - ${Price}", quote.Symbol, quote.Price);
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Data successfully saved to PostgreSQL.");
                
                await AnalyzeChangesAsync(symbols);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical runtime error StockProcessorService");
            }
        }

        private async Task AnalyzeChangesAsync(List<string> symbols)
        {
            _logger.LogInformation("Analysis of price changes over the last hour...");

            foreach (var symbol in symbols)
            {
                var latest = await _dbContext.Quotes
                    .Where(q => q.Symbol == symbol)
                    .OrderByDescending(q => q.RecordedAt)
                    .FirstOrDefaultAsync();

                if (latest == null) continue;

                var old = await _dbContext.Quotes
                    .Where(q => q.Symbol == symbol && q.RecordedAt < latest.RecordedAt.AddHours(-1))
                    .OrderByDescending(q => q.RecordedAt)
                    .FirstOrDefaultAsync();

                if (old != null)
                {
                    decimal priceChange = latest.Price - old.Price;
                    decimal percentChange = (priceChange / old.Price) * 100;

                    _logger.LogInformation("Analiz | {Symbol}: {Change:F2}$ ({Percent:F2}%) Percent {Time}",
                        symbol, priceChange, percentChange, old.RecordedAt.ToLocalTime());
                }
                else
                {
                    _logger.LogDebug("{Symbol}: Not enough data for analysis.", symbol);
                }
            }
        }
    }
}