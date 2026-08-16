using WatchListScreening.Infrastructure;
using WatchListScreening.Application;
using WatchListScreening.API.Middlewares;
using WatchListScreening.API.Consumers;
using Serilog;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;

// 1. Serilog Konfigürasyonu
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/watchlist-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("WatchList Screening API başlatılıyor...");

    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u host'a entegre et
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    
    // Swagger/OpenAPI konfigürasyonu
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // CORS Konfigürasyonu (MVC paneli vs için)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Layer bağımlılıkları
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplicationServices();

    // 3. Hangfire Konfigürasyonu
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
        
    builder.Services.AddHangfireServer(options => {
        options.WorkerCount = 1; // Sadece Job trigger edecek, ağır iş yapmayacak
    });

    // 4. MassTransit & RabbitMQ Konfigürasyonu
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<HarvestResultConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });

            cfg.ReceiveEndpoint("harvest-results", e =>
            {
                e.ConfigureConsumer<HarvestResultConsumer>(context);
            });
        });
    });

    var app = builder.Build();

    // 2. Middleware'ler (Pipeline)
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WatchList Screening API v1"));
    }

    app.UseSerilogRequestLogging(); // Request loglarını detaylı yazar

    app.UseHttpsRedirection();

    app.UseCors("AllowAll"); // CORS'u aktifleştir

    app.UseAuthorization();
    
    // Hangfire Dashboard (Production'da authorization eklenmelidir!)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        // Ignore auth for dev purposes (In production use IDashboardAuthorizationFilter)
        Authorization = new [] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() } 
    });

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılırken kritik bir hata oluştu!");
}
finally
{
    Log.CloseAndFlush();
}
