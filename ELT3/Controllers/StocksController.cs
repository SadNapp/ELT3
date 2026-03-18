using ELT3.Data;
using ELT3.Models;
using ELT3.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELT3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly YahooApiClient _yahooApiClient;

    public StocksController(AppDbContext context, YahooApiClient yahooApiClient)
    {
        _context = context;
        _yahooApiClient = yahooApiClient;
    }

    [HttpGet("search/{symbol}")]
    public async Task<ActionResult<StockQuote>> SearchNewStock(string symbol)
    {
        symbol = symbol.ToUpper().Trim();

        // 1. First, we try to find the promotion in the API directly by this word
        var result = await _yahooApiClient.GetStockQuotesAsync(new[] { symbol });
        var quote = result.FirstOrDefault();

        // 2. If not found, try to find the ticker through search
        if (quote == null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var url = $"https://query2.finance.yahoo.com/v1/finance/search?q={symbol}";

            try
            {
                var response = await client.GetAsync(url);
                var data = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

                if (data.TryGetProperty("quotes", out var quotes) && quotes.ValueKind == System.Text.Json.JsonValueKind.Array && quotes.GetArrayLength() > 0)
                {
                    var bestMatchSymbol = quotes[0].GetProperty("symbol").GetString();
                    if (!string.IsNullOrEmpty(bestMatchSymbol))
                    {
                        var retryResult = await _yahooApiClient.GetStockQuotesAsync(new[] { bestMatchSymbol });
                        quote = retryResult.FirstOrDefault();
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        if (quote != null)
        {
            quote.RecordedAt = DateTime.UtcNow;
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();
            return Ok(quote);
        }

        return NotFound($"Could not find any stock matching '{symbol}'");
    }

    [HttpGet("autocomplete/{query}")]
    public async Task<ActionResult> GetSuggestions(string query)
    {
        // For speed, we leave HttpClient, but for the future it is better to use IHttpClientFactory
        using var client = new HttpClient();
        var url = $"https://query2.finance.yahoo.com/v1/finance/search?q={query}";

        try
        {
            // Yahoo API is protected from robots, sometimes a User-Agent is required
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            var response = await client.GetAsync(url);
            var data = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

            if (data.TryGetProperty("quotes", out var quotes))
            {
                return Ok(quotes);
            }
            return NotFound("No quotes found.");
        }
        catch (Exception)
        {
            return BadRequest("Failed to get suggestions from Yahoo.");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockQuote>>> GetLatestQuotes()
    {
        var latestQuotes = await _context.Quotes
            .GroupBy(q => q.Symbol)
            .Select(g => g.OrderByDescending(q => q.RecordedAt).First())
            .ToListAsync();


        return Ok(latestQuotes);
    }

    [HttpGet("{symbol}")]
    public async Task<ActionResult<IEnumerable<StockQuote>>> GetHistory(string symbol)
    {
        var history = await _context.Quotes
            .Where(q => q.Symbol.ToUpper() == symbol.ToUpper())
            .OrderByDescending(q => q.RecordedAt)
            .Take(100) 
            .ToListAsync();

        if (!history.Any()) return NotFound();

        return Ok(history);
    }
}