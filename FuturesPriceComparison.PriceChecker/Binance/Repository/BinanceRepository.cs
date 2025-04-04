using System.Data.Common;
using FuturesPriceComparison.PriceChecker.Binance.Models.ServiceModels;
using FuturesPriceComparison.PriceChecker.Constants;
using FuturesPriceComparison.PriceChecker.Repositories;
using Polly;

namespace FuturesPriceComparison.PriceChecker.Binance.Repository;

public sealed class BinanceRepository(
    [FromKeyedServices(PoliciesNames.DbPolicy)]
    ResiliencePipeline retryPolicy,
    PostgresRepository postgresRepository)
{
    public async Task<DbTransaction> CreateTransaction(CancellationToken cancellationToken = default)
    {
        return await postgresRepository.BeginTransactionAsync(cancellationToken);
    }

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
        DbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async ct =>
            await postgresRepository.SaveNewPrice(
                futuresId,
                price,
                timestamp,
                transaction,
                ct),
            cancellationToken);
    }

    public async Task SaveDifference(
        int firstFuturesId,
        int secondFuturesId,
        decimal difference,
        DateTime timestampUtc,
        DbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async ct =>
            await postgresRepository.SaveDifference(
                firstFuturesId,
                secondFuturesId,
                difference,
                timestampUtc,
                transaction,
                ct),
            cancellationToken);
    }
}
