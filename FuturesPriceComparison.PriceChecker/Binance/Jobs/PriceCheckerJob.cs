using FuturesPriceComparison.Models.ServiceModels;
using FuturesPriceComparison.PriceChecker.Binance.Repository;
using FuturesPriceComparison.PriceChecker.Exceptions;
using FuturesPriceComparison.PriceChecker.Interfaces;
using Npgsql;
using Quartz;

namespace FuturesPriceComparison.PriceChecker.Binance.Jobs;

[DisallowConcurrentExecution]
public class PriceCheckerJob(
    IDateTmeProvider dateTmeProvider,
    IExchangeClient binanceClient,
    BinancePostgresRepository binancePostgresRepository,
    ILogger<PriceCheckerJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        NpgsqlTransaction? transaction = null;
        try
        {
            var pairs = await binancePostgresRepository.GetPairsToCheck(cancellationToken);
            foreach (var pairToCheck in pairs)
            {
                await using (transaction = await binancePostgresRepository.CreateTransaction(cancellationToken))
                {
                    var actualPrices = await GetActualPrices(pairToCheck);
                    var difference = actualPrices.FirstPrice - actualPrices.SecondPrice;

                    await binancePostgresRepository.SaveNewPrice(
                        actualPrices.FirstFuturesId,
                        actualPrices.FirstPrice,
                        actualPrices.TimeStamp,
                        transaction,
                        cancellationToken);
                    await binancePostgresRepository.SaveNewPrice(
                        actualPrices.SecondFuturesId,
                        actualPrices.SecondPrice,
                        actualPrices.TimeStamp,
                        transaction,
                        cancellationToken);

                    await binancePostgresRepository.SaveDifference(
                        actualPrices.FirstFuturesId,
                        actualPrices.SecondFuturesId,
                        difference,
                        actualPrices.TimeStamp,
                        transaction,
                        cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
            }

            logger.LogInformation("Price check job finished");
        }
        catch (Exception e)
        {
            if (transaction != null)
                await transaction.RollbackAsync(cancellationToken);
            logger.LogError(e, "Error during checking price");
        }
        finally
        {
            if (transaction != null)
                await transaction.DisposeAsync();
        }
    }

    private async Task<ActualPrices> GetActualPrices(PairToCheck p)
    {
        var firstPriceTask = binanceClient.GetPriceForActive(p.FirstSymbol);
        var secondPriceTask = binanceClient.GetPriceForActive(p.SecondSymbol);
        await Task.WhenAll(firstPriceTask, secondPriceTask);
        var firstPrice = await firstPriceTask;
        var secondPrice = await secondPriceTask;

        LastFuturesPrice? firstPriceDb = null;
        if (firstPrice is null)
        {
            logger.LogError("Failed to get price for {symbol}", p.FirstSymbol);
            firstPriceDb = await binancePostgresRepository.GetLastAvailablePrice(p.FirstId);
        }

        LastFuturesPrice? secondPriceDb = null;
        if (secondPrice is null)
        {
            logger.LogError("Failed to get price for {symbol}", p.SecondSymbol);
            secondPriceDb = await binancePostgresRepository.GetLastAvailablePrice(p.SecondId);
        }

        return new ActualPrices(
            FirstFuturesId: p.FirstId,
            SecondFuturesId: p.SecondId,
            FirstPrice: firstPrice?.Price ?? firstPriceDb?.Price ?? throw new NoAvailableInfoException(p.FirstName),
            SecondPrice: secondPrice?.Price ?? secondPriceDb?.Price ?? throw new NoAvailableInfoException(p.SecondName),
            TimeStamp: dateTmeProvider.TimestampUtc);
    }

    private record ActualPrices(
        int FirstFuturesId,
        int SecondFuturesId,
        decimal FirstPrice,
        decimal SecondPrice,
        DateTime TimeStamp);
}
