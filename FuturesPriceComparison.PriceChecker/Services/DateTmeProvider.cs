using FuturesPriceComparison.PriceChecker.Interfaces;

namespace FuturesPriceComparison.PriceChecker.Services;

public class DateTmeProvider : IDateTmeProvider
{
    public DateTime TimestampUtc => DateTime.UtcNow;
}
