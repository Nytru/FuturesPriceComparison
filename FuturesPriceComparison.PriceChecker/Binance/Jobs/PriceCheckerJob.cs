using FuturesPriceComparison.Models.ServiceModels;
using FuturesPriceComparison.PriceChecker.Binance.Repository;
using FuturesPriceComparison.PriceChecker.Interfaces;
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
        try
        {
            var pairs = await binancePostgresRepository.GetPairsToCheck();
            foreach (var pairToCheck in pairs)
            {
                var actualPrices = await GetActualPrices(pairToCheck);
                var difference = actualPrices.FirstPrice - actualPrices.SecondPrice;

                var t1 = binancePostgresRepository.SaveNewPrice(
                    actualPrices.FirstFuturesId,
                    actualPrices.FirstPrice,
                    actualPrices.TimeStamp);
                var t2= binancePostgresRepository.SaveNewPrice(
                    actualPrices.SecondFuturesId,
                    actualPrices.SecondPrice,
                    actualPrices.TimeStamp);

                var t3 = binancePostgresRepository.SaveDifference(
                    actualPrices.FirstFuturesId,
                    actualPrices.SecondFuturesId,
                    difference,
                    actualPrices.TimeStamp);
                await Task.WhenAll(t1, t2, t3);
            }

            logger.LogInformation("Price check job finished successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error during checking price");
        }
    }

    private async Task<ActualPrices> GetActualPrices(PairToCheck p)
    {
        var firstPriceTask = binanceClient.GetPriceForActive(p.FirstSymbol);
        var secondPriceTask = binanceClient.GetPriceForActive(p.SecondSymbol);
        await Task.WhenAll(firstPriceTask, secondPriceTask);
        var firstPrice = await firstPriceTask;
        var secondPrice = await secondPriceTask;

        LastFuturesPrice firstPriceDb = null!;
        if (firstPrice is null)
        {
            firstPriceDb = await binancePostgresRepository.GetLastAvailablePrice(p.FirstId);
        }

        LastFuturesPrice secondPriceDb = null!;
        if (secondPrice is null)
        {
            secondPriceDb = await binancePostgresRepository.GetLastAvailablePrice(p.SecondId);
        }

        return new ActualPrices(
            FirstFuturesId: p.FirstId,
            SecondFuturesId: p.SecondId,
            FirstPrice: firstPrice?.Price ?? firstPriceDb.Price,
            SecondPrice: secondPrice?.Price ?? secondPriceDb.Price,
            TimeStamp: dateTmeProvider.TimestampUtc);
    }

    private record ActualPrices(
        int FirstFuturesId,
        int SecondFuturesId,
        decimal FirstPrice,
        decimal SecondPrice,
        DateTime TimeStamp);
}
