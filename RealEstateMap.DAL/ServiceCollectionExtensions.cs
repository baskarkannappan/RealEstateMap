using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateMap.DAL.Configuration;
using RealEstateMap.DAL.Connection;
using RealEstateMap.DAL.Execution;
using RealEstateMap.DAL.Repositories;

namespace RealEstateMap.DAL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealEstateDal(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IDbExecutor, DapperDbExecutor>();
        services.AddScoped<IHouseRepository, HouseRepository>();
        return services;
    }
}
