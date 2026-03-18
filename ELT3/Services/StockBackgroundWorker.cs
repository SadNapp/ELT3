using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ELT3.Services;

public class StockBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockBackgroundWorker> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // interval 5 minutes

    public StockBackgroundWorker(IServiceProvider serviceProvider, ILogger<StockBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Background Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Background task executed at: {time}", DateTimeOffset.Now);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<StockProcessorService>();
                    await processor.ProcessAsync(); // call method
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing stock update task.");
            }

            // wait 5 min before the next cycle
            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("Stock Background Worker is stopping.");
    }
}