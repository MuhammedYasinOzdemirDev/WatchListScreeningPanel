using WatchListScreening.Infrastructure;
using WatchListScreening.Application;
using WatchListScreening.API.Middlewares;
using Serilog;

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
