using FuturesPriceComparison.PriceChecker.Interfaces;
using Microsoft.Extensions.Options;

namespace FuturesPriceComparison.PriceChecker.Binance.Services;

public class BinanceClient : IExchangeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BinanceClient> _logger;

    public BinanceClient(
        HttpClient httpClient,
        IOptions<BinanceApiOptions> options,
        ILogger<BinanceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = options.Value.BaseUrl;
    }

    public async Task<Ticker?> GetPriceForActive(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            const string urlTemplate = "/fapi/v2/ticker/price?symbol={0}";
            var result = await _httpClient.GetAsync(string.Format(urlTemplate, symbol), cancellationToken);
            result.EnsureSuccessStatusCode();
            var exResult = await result.Content.ReadFromJsonAsync<Ticker>(cancellationToken);
            return exResult;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to parse ticker");
            return null;
        }
    }
}
