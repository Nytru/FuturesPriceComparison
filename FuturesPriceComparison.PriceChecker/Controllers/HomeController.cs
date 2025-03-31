using FuturesPriceComparison.PriceChecker.Interfaces;
using FuturesPriceComparison.PriceChecker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FuturesPriceComparison.PriceChecker.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
    private readonly IExchangeClient _binanceClient;
    private readonly PostgresRepository _postgresRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IExchangeClient binanceClient,
        PostgresRepository postgresRepository,
        ILogger<HomeController> logger)
    {
        _binanceClient = binanceClient;
        _postgresRepository = postgresRepository;
        _logger = logger;
    }

    [HttpGet("foo")]
    public async Task<IActionResult> Foo(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Foo");
        var r = await _postgresRepository.GetLastAvailablePrice(3);
        return Ok(r);
    }
}
