namespace FuturesPriceComparison.PriceChecker.Exceptions;

public class NoAvailableInfoException(string message) : Exception($"Failed to get any info about {message}");
