using FuturesPriceComparison.Models.ServiceModels;
using FuturesPriceComparison.PriceChecker.Constants;
using FuturesPriceComparison.PriceChecker.Repositories;
using Polly;

namespace FuturesPriceComparison.PriceChecker.Binance.Repository;

public class BinancePostgresRepository(
    [FromKeyedServices(PoliciesNames.PostgresPolicy)]
    ResiliencePipeline retryPolicy,
    PostgresRepository postgresRepository)
{
    public async Task<IEnumerable<PairToCheck>> GetPairsToCheck(CancellationToken cancellationToken = default)
    {
        return await retryPolicy.ExecuteAsync(async ct =>
            await postgresRepository.GetPairsToCheck(ExchangesNames.Binance, ct),
            cancellationToken);
    }

    public async Task<LastFuturesPrice> GetLastAvailablePrice(
        int firmId,
        CancellationToken cancellationToken = default)
    {
        return await retryPolicy.ExecuteAsync(async ct =>
            await postgresRepository.GetLastAvailablePrice(firmId, ct),
            cancellationToken);
    }

    public async Task SaveNewPrice(
        int futuresId,
        decimal price,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async ct =>
            await postgresRepository.SaveNewPrice(futuresId, price, timestamp, ct),
            cancellationToken);
    }

    public async Task SaveDifference(
        int firstFuturesId,
        int secondFuturesId,
        decimal difference,
        DateTime timestampUtc,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async ct =>
            await postgresRepository.SaveDifference(firstFuturesId, secondFuturesId, difference, timestampUtc, ct),
            cancellationToken);
    }
}
