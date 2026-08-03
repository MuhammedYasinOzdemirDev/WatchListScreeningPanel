using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Infrastructure.Data;
using WatchListScreening.Infrastructure.Data.Repositories;

namespace WatchListScreening.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISanctionEntryRepository, SanctionEntryRepository>();
 
        return services;
    }
}
