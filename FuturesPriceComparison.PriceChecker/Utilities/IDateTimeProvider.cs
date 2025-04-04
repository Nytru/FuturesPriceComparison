namespace FuturesPriceComparison.PriceChecker.Utilities;

public interface IDateTimeProvider
{
    DateTime TimestampUtc { get; }
}
