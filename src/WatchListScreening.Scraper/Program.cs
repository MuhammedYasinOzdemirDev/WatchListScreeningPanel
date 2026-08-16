using MassTransit;
using WatchListScreening.Application.Interfaces.Scraping;
using WatchListScreening.Scraper.Cleaners.Pipeline;
using WatchListScreening.Scraper.ConcreteScrapers;
using WatchListScreening.Scraper.Factory;
using WatchListScreening.Scraper.Workers;
using WatchListScreening.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Extensions.Http;

var builder = Host.CreateApplicationBuilder(args);

// 1. Infrastructure (Database) - Clean Architecture (From Configuration)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DefaultConnection string is not configured.");
}
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// 2. HttpClients ve Polly (Retry Policy)
// Hata (5xx, 408) durumlarında 3 kez yeniden deneme poliçesi
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddHttpClient<ISourceScraper, OfacScraper>()
    .AddPolicyHandler(retryPolicy);

// 3. Concrete Scrapers Registration (for Collection Injection resolution)
// Note: OfacScraper is already registered as ISourceScraper via AddHttpClient above.
builder.Services.AddScoped<ISourceScraper, MasakScraper>();

// 4. Core Services
builder.Services.AddScoped<IScraperFactory, ScraperFactory>();

// 5. Pipeline Registration (Scrutor kullanılabilir, manuel olarak kaydediyorum)
builder.Services.AddSingleton<ICleaningStep, HtmlEntityDecoderStep>();
builder.Services.AddSingleton<ICleaningStep, UnicodeNormalizerStep>();
builder.Services.AddSingleton<ICleaningStep, NameNormalizerStep>();
builder.Services.AddSingleton<ICleaningStep, NameSplitterStep>();
builder.Services.AddSingleton<ICleaningStep, CategoryClassifierStep>();
builder.Services.AddSingleton<ICleaningStep, HashGeneratorStep>();
builder.Services.AddSingleton<IDataCleaner, CleaningPipeline>();

// 6. MassTransit (RabbitMQ) - Clean Architecture (From Configuration)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<HarvestWorker>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rmqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rmqUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rmqPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rmqHost, "/", h =>
        {
            h.Username(rmqUser);
            h.Password(rmqPass);
        });

        cfg.ReceiveEndpoint("harvest-commands", e =>
        {
            e.ConfigureConsumer<HarvestWorker>(context);
        });
    });
});

var host = builder.Build();
host.Run();
