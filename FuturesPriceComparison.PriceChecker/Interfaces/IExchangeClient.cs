using FuturesPriceComparison.Models.ServiceModels;

namespace FuturesPriceComparison.PriceChecker.Interfaces;

public interface IExchangeClient
{
    public Task<Ticker?> GetPriceForActive(
        string symbol,
        CancellationToken cancellationToken = default);
}
