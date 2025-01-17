using LiteDB.Tables.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LiteDB.Tables;
public static class LiteDbTablesServiceExtensions
{
    public static IServiceCollection AddLiteDbTables(
        this IServiceCollection services,
        Action<LiteDBTablesConfig> configureOptions)
    {
        // Register the default configuration object
        var config = new LiteDBTablesConfig();

        // Configure the options passed into the service registration
        configureOptions?.Invoke(config);

        // Register the configuration with the DI container
        services.AddSingleton(config);

        //// Use IOptions<T> to access the config wherever needed
        services.Configure<LiteDBTablesConfig>(options =>
        {
            options.ConnectionString = config.ConnectionString;
        });

        // Register services 
        services.AddSingleton<LiteDbService>();
        return services;
    }
}
