using Microsoft.Extensions.DependencyInjection;
using WatchListScreening.Application.Interfaces.Services;
using WatchListScreening.Application.Services;

namespace WatchListScreening.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISanctionEntryService, SanctionEntryService>();
        services.AddScoped<IScreeningService, ScreeningService>();
        services.AddScoped<IScreeningResultService, ScreeningResultService>();
        
        services.AddSingleton<MatchingEngine>();

        return services;
    }
}
