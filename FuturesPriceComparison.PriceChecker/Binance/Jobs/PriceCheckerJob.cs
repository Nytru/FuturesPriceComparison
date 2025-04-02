using FuturesPriceComparison.PriceChecker.Binance.Models.ServiceModels;
using FuturesPriceComparison.PriceChecker.Binance.Repository;
using FuturesPriceComparison.PriceChecker.Utilities;
using Quartz;

namespace FuturesPriceComparison.PriceChecker.Binance.Jobs;

[DisallowConcurrentExecution]
public class PriceCheckerJob(
    IDateTimeProvider dateTimeProvider,
    IExchangeClient binanceClient,
    BinanceRepository binanceRepository,
    ILogger<PriceCheckerJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        try
        {
            var pairs = await binanceRepository.GetPairsToCheck(cancellationToken);
            foreach (var pairToCheck in pairs)
            {
                var transaction = await binanceRepository.CreateTransaction(cancellationToken);
                try
                {
                    var actualPrices = await GetActualPrices(pairToCheck);
                    var difference = actualPrices.FirstPrice - actualPrices.SecondPrice;

                    await binanceRepository.SaveNewPrice(
                        actualPrices.FirstFuturesId,
                        actualPrices.FirstPrice,
                        actualPrices.TimeStamp,
                        transaction,
                        cancellationToken);
                    await binanceRepository.SaveNewPrice(
                        actualPrices.SecondFuturesId,
                        actualPrices.SecondPrice,
                        actualPrices.TimeStamp,
                        transaction,
                        cancellationToken);

                    await binanceRepository.SaveDifference(
                        actualPrices.FirstFuturesId,
                        actualPrices.SecondFuturesId,
                        difference,
                        actualPrices.TimeStamp,
                        transaction,
                        cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogError(e, "Error while trying check price");
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            }

            logger.LogInformation("Price check job finished");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting info to check");
        }
    }

    private async Task<ActualPrices> GetActualPrices(PairToCheck p)
    {
        var firstPriceTask = PriceForActive(p.FirstSymbol, p.FirstId);
        var secondPriceTask = PriceForActive(p.SecondSymbol, p.SecondId);
        await Task.WhenAll(firstPriceTask, secondPriceTask);
        var firstPrice = await firstPriceTask;
        var secondPrice = await secondPriceTask;

        return new ActualPrices(
            FirstFuturesId: p.FirstId,
            SecondFuturesId: p.SecondId,
            FirstPrice: firstPrice,
            SecondPrice: secondPrice,
            TimeStamp: dateTimeProvider.TimestampUtc);
    }

    private async Task<decimal> PriceForActive(string symbol, int futuresId)
    {
        var price = await binanceClient.GetPriceForActive(symbol);
        if (price is not null)
            return price.Price;

        var dbPrice = await binanceRepository.GetLastAvailablePrice(futuresId);
        return dbPrice.Price;
    }

    private record ActualPrices(
        int FirstFuturesId,
        int SecondFuturesId,
        decimal FirstPrice,
        decimal SecondPrice,
        DateTime TimeStamp);
}
