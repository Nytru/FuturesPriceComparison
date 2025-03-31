namespace FuturesPriceComparison.PriceChecker.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection ConfigureByName<T>(this IServiceCollection services, IConfiguration configuration)
        where T : class
    {
        return services.Configure<T>(configuration.GetRequiredSection(typeof(T).Name));
    }
}
