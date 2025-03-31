namespace FuturesPriceComparison.PriceChecker.Exceptions;

public class JsonParsingException<T> : Exception
{
    // TODO: improve information
    public JsonParsingException(string message) :  base(message)
    {
        
    }
}