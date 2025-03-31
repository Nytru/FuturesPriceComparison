namespace FuturesPriceComparison.PriceChecker.Interfaces;

public interface IExchangeClient
{
    public Task<IReadOnlyCollection<ExchangeSymbol>> GetFuturesForPair(
        string symbol,
        CancellationToken cancellationToken = default);

    public Task<Ticker?> GetPriceForActive(
        string symbol,
        CancellationToken cancellationToken = default);
}

public record ExchangeSymbol(string Pair, string Symbol);

public record Ticker(decimal Price, string Symbol);