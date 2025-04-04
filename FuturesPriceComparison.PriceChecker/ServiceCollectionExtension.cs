using FluentMigrator.Runner;
using FuturesPriceComparison.PriceChecker.Exceptions;
using FuturesPriceComparison.PriceChecker.Migrations;

namespace FuturesPriceComparison.PriceChecker;

public static class ServiceCollectionExtension
{
    public static IServiceCollection ConfigureByName<T>(this IServiceCollection services, IConfiguration configuration)
        where T : class
    {
        return services.Configure<T>(configuration.GetRequiredSection(typeof(T).Name));
    }

    public static IServiceCollection AddNpgsql(this IServiceCollection services, IConfiguration configuration)
    {
        const string envName = "POSTGRES_CONNECTION_STRING";
        var value = configuration.GetValue<string>(envName);
        if (value is null)
            throw new MissingConfigException($"{envName} env is missing");

        return services
            .AddNpgsqlDataSource(value)
            .AddMigrations(configuration, envName);
    }

    private static IServiceCollection AddMigrations(
        this IServiceCollection services,
        IConfiguration configuration,
        string envName) => services
        .AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddPostgres()
            .WithGlobalConnectionString(configuration.GetValue<string>(envName))
            .ScanIn(typeof(InitialMigration).Assembly).For.Migrations()
            .ScanIn(typeof(FillUpMigration).Assembly).For.Migrations());
}
