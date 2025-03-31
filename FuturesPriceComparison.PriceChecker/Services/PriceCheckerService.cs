using FuturesPriceComparison.PriceChecker.Interfaces;
using FuturesPriceComparison.PriceChecker.Repositories;
using Npgsql;

namespace FuturesPriceComparison.PriceChecker.Services;

public class PriceCheckerService
{
    private readonly IExchangeClient _binanceClient;
    private readonly PostgresRepository _postgresRepository;

    public PriceCheckerService(IExchangeClient binanceClient, PostgresRepository postgresRepository)
    {
        _binanceClient = binanceClient;
        _postgresRepository = postgresRepository;
    }

    public async Task CheckPrice()
    {
        var pairs = await _postgresRepository.GetPairsToCheck();
        foreach (var pairToCheck in pairs)
        {
            var actualPrices = await GetActualPrices(pairToCheck);
            var difference = actualPrices.FirstPrice - actualPrices.SecondPrice;

            var t1 = _postgresRepository.SaveNewPrice(
                actualPrices.FirstFuturesId,
                actualPrices.FirstPrice,
                actualPrices.TimeStamp);
            var t2= _postgresRepository.SaveNewPrice(
                actualPrices.SecondFuturesId,
                actualPrices.SecondPrice,
                actualPrices.TimeStamp);

            var t3 = _postgresRepository.SaveDifference(
                actualPrices.FirstFuturesId,
                actualPrices.SecondFuturesId,
                difference,
                actualPrices.TimeStamp);
            await Task.WhenAll(t1, t2, t3);
        }
    }

    private async Task<ActualPrices> GetActualPrices(PairToCheck p)
    {
        var firstPriceTask = _binanceClient.GetPriceForActive(p.FirstSymbol);
        var secondPriceTask = _binanceClient.GetPriceForActive(p.SecondSymbol);
        await Task.WhenAll(firstPriceTask, secondPriceTask);
        var firstPrice = await firstPriceTask;
        var secondPrice = await secondPriceTask;

        LastFuturesPrice firstPriceDb = null!;
        if (firstPrice is null)
        {
            firstPriceDb = await _postgresRepository.GetLastAvailablePrice(p.FirstId);
        }

        LastFuturesPrice secondPriceDb = null!;
        if (secondPrice is null)
        {
            secondPriceDb = await _postgresRepository.GetLastAvailablePrice(p.SecondId);
        }

        var timeStamp = DateTime.UtcNow;

        return new ActualPrices
        {
            FirstFuturesId = p.FirstId,
            SecondFuturesId = p.SecondId,
            FirstPrice = firstPrice?.Price ?? firstPriceDb.Price,
            SecondPrice = secondPrice?.Price ?? secondPriceDb.Price,
            TimeStamp = timeStamp,
        };
    }
}

public record ActualPrices
{
    public int FirstFuturesId { get; init; }
    public int SecondFuturesId { get; init; }
    public decimal FirstPrice { get; init; }
    public decimal SecondPrice { get; init; }
    public DateTime TimeStamp { get; init; }
}
