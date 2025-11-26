using Microsoft.Extensions.DependencyInjection;
using ELT3.Services;
using ELT3.Models; 
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using ELT3.Data;

var builder = WebApplication.CreateBuilder(args);

// get Connection String
var connectionString = builder.Configuration.GetConnectionString("PgsqlConnection");

// register AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); 


builder.Services.AddSingleton<YahooApiClient>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var scopedServices = scope.ServiceProvider;
    var dbContext = scopedServices.GetRequiredService<AppDbContext>();
    
    await dbContext.Database.EnsureCreatedAsync(); 

    // started request, passing on Scoped-provider
    await RunPriceQuery(scopedServices);
}


async Task RunPriceQuery(IServiceProvider services)
{
    // get service in Scope
    var client = services.GetRequiredService<YahooApiClient>();
    var dbContext = services.GetRequiredService<AppDbContext>();

    // list shares for request
    List<string> symbols = new List<string> { "AAPL", "MSFT", "GOOGL", "TSLA", "AMZN", "NVDA", "META" };

    Console.WriteLine($"Request stock prices for: {string.Join(", ", symbols)}");
    Console.WriteLine("-----------------------------------------------------");

    try
    {
        // 1. get Data
        var quotes = await client.GetStockQuotesAsync(symbols);
        var now = DateTime.UtcNow; // using UTC for Record in DB

        // TRANSFORMATION AND INSERTING INTO CONTEXT
        foreach (var quote in quotes)
        {
            quote.RecordedAt = now; // Recordin UTC time, when the data was saved
            dbContext.Quotes.Add(quote);

            Console.ForegroundColor = ConsoleColor.Green;
            // we convert the time to local for convenience
            Console.WriteLine($"| {quote.Symbol,-5} | Price: ${quote.Price,8:F2} " +
                              $"| Change: {quote.ChangesPercentage,6:F2}% | Last Update: {quote.LastUpdateTime.ToLocalTime():yyyy-M-d dddd HH:mm:ss}|"); 
        }

        var foundSymbols = quotes.Select(q => q.Symbol);
        var notFound = symbols.Except(foundSymbols);

        if (notFound.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nNo data found for: {string.Join(", ", notFound)}");
        }

        //Save Data
        await dbContext.SaveChangesAsync();
        Console.WriteLine("\n>>SUCCESS: Data successfully saved to PostgreSQL.");

        //Analizz
        await AnalyzeChanges(dbContext, symbols);

    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Critical error: An error occurred while saving the entity changes. See the inner exception for details.");
        // Output Inner Exception message for diagnostics, if possible
        Console.WriteLine($"Details: {ex.InnerException?.Message ?? ex.Message}");
    }
    finally
    {
        Console.ResetColor();
        Console.WriteLine("-----------------------------------------------------");
    }
}


async Task AnalyzeChanges(AppDbContext dbContext, List<string> symbols)
{
    Console.WriteLine("\n>> ANALYSIS: Price changes in the last few hours (if old data is available):");
    Console.ForegroundColor = ConsoleColor.Cyan;

    foreach (var symbol in symbols)
    {
        // We get the last record
        var latest = await dbContext.Quotes
            .Where(q => q.Symbol == symbol)
            .OrderByDescending(q => q.RecordedAt)
            .FirstOrDefaultAsync();

        if (latest == null) continue;

        // We get an old recording (the one that was about an hour ago)
        var old = await dbContext.Quotes
            .Where(q => q.Symbol == symbol && q.RecordedAt < latest.RecordedAt.AddHours(-1)) 
            .OrderByDescending(q => q.RecordedAt)
            .FirstOrDefaultAsync();

        if (old != null)
        {
            decimal priceChange = latest.Price - old.Price;
            decimal percentChange = (priceChange / old.Price) * 100;

            // Output data from DB (which is UTC) to local time for console
            Console.WriteLine(
                $"  {symbol,-5}: {priceChange:F2} $ ({percentChange:F2}%) change from {old.RecordedAt.ToLocalTime():yyyy-MM-dd HH:mm}"); 
        }
        else
        {
            Console.WriteLine($"  {symbol,-5}: (Not enough historical data for comparison)");
        }
    }

    Console.ResetColor();
}