namespace FuturesPriceComparison.PriceChecker.Interfaces;

public interface IExchangeClient
{
    public Task<Ticker?> GetPriceForActive(
        string symbol,
        CancellationToken cancellationToken = default);
}

public record Ticker(decimal Price, string Symbol);
