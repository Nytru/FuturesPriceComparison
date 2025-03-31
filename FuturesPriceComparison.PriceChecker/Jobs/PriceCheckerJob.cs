using FuturesPriceComparison.PriceChecker.Services;
using Quartz;

namespace FuturesPriceComparison.PriceChecker.Jobs;

[DisallowConcurrentExecution]
public class PriceCheckerJob :  IJob
{
    private readonly PriceCheckerService _priceCheckerService;

    public PriceCheckerJob(PriceCheckerService priceCheckerService)
    {
        _priceCheckerService = priceCheckerService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await _priceCheckerService.CheckPrice();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}