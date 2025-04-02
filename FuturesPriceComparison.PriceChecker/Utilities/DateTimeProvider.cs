namespace FuturesPriceComparison.PriceChecker.Utilities;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime TimestampUtc => DateTime.UtcNow;
}
