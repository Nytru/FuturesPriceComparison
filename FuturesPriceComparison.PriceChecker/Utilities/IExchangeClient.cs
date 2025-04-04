using FuturesPriceComparison.PriceChecker.Binance.Models.ServiceModels;

namespace FuturesPriceComparison.PriceChecker.Utilities;

public interface IExchangeClient
{
    public Task<Ticker?> GetPriceForActive(
        string symbol,
        CancellationToken cancellationToken = default);
}
