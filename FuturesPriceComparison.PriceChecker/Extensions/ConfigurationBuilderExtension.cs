namespace FuturesPriceComparison.PriceChecker.Extensions;

public static class ConfigurationBuilderExtension
{
    public static IConfigurationBuilder AddAppSettings(this IConfigurationBuilder configuration)
    {
        var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{currentEnv}.json", optional: true, reloadOnChange: false);
    }
}