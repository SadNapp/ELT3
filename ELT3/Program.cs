using ELT3.Data;
using ELT3.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// 1. LOGIN SETTINGS
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // we can clear this string for all EF Core logs, but we want to keep the errors
    // Hidem SQL Info, leave erro
    .Filter.ByExcluding("SourceContext = 'Microsoft.EntityFrameworkCore.Database.Command' and @Level = 'Information'")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/etl-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. reg Services
var connectionString = builder.Configuration.GetConnectionString("PgsqlConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddCors(option =>
{
    option.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddHttpClient<YahooApiClient>();
builder.Services.AddScoped<StockProcessorService>();
builder.Services.AddHostedService<StockBackgroundWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseDefaultFiles();
app.UseStaticFiles();

// 3. config HTTP-PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock ETL API V1");
        c.RoutePrefix = "swagger";
    });
}

app.MapControllers(); // To make the StocksController

// 4. BASE INITIALIZATION AND STARTUP ETL3
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();

        try
        {
            Log.Information("Applying migrations...");
            // ЗАМІСТЬ EnsureCreatedAsync використовуємо MigrateAsync
            await context.Database.MigrateAsync();

            var processor = services.GetRequiredService<StockProcessorService>();
            await processor.ProcessAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database connection or migration failed. The app will continue running without DB, but some features might not work.");
        }
    }

    Log.Information("Initialization completed successfully. Starting the web server...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The app crashed during initialization.");
}
finally
{
    Log.Information("App Stop...");
    Log.CloseAndFlush();
}