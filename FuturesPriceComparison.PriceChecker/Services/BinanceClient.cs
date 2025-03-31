using FuturesPriceComparison.PriceChecker.Exceptions;
using FuturesPriceComparison.PriceChecker.Interfaces;
using Microsoft.Extensions.Options;

namespace FuturesPriceComparison.PriceChecker.Services;

// TODO: add retry policies
public class BinanceClient : IExchangeClient
{
    private readonly HttpClient _httpClient;

    public BinanceClient(HttpClient httpClient, IOptions<BinanceApiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = options.Value.BaseUrl;
    }

    /// <exception cref="T:System.Net.Http.HttpRequestException">The HTTP response is unsuccessful.</exception>
    /// <exception cref="JsonParsingException{T}">Failed to parse response</exception>
    public async Task<IReadOnlyCollection<ExchangeSymbol>> GetFuturesForPair(string symbol, CancellationToken cancellationToken = default)
    {
        const string url = "fapi/v1/exchangeInfo";
        var result = await _httpClient.GetAsync(url, cancellationToken);
        result.EnsureSuccessStatusCode();
        var exResult = await result.Content.ReadFromJsonAsync<ExchangeInfo>(cancellationToken);
        if (exResult is null)
            throw new JsonParsingException<ExchangeInfo>("Failed to parse exchangeInfo");

        return exResult.Symbols
            .Where(s => s.Pair.Contains(symbol, StringComparison.InvariantCultureIgnoreCase))
            .ToArray();
    }

    /// <exception cref="T:System.Net.Http.HttpRequestException">The HTTP response is unsuccessful.</exception>
    /// <exception cref="JsonParsingException{T}">Failed to parse response</exception>
    public async Task<Ticker?> GetPriceForActive(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            const string urlTemplate = "/fapi/v2/ticker/price?symbol={0}";
            var result = await _httpClient.GetAsync(string.Format(urlTemplate, symbol), cancellationToken);
            result.EnsureSuccessStatusCode();
            var exResult = await result.Content.ReadFromJsonAsync<Ticker>(cancellationToken);
            if (exResult is null)
                throw new JsonParsingException<Ticker>("Failed to parse ticker");

            return exResult;
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to parse ticker");
            return null;
        }
    }

    private record ExchangeInfo(List<ExchangeSymbol> Symbols);
}
