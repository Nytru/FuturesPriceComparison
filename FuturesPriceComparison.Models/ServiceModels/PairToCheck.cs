namespace FuturesPriceComparison.Models.ServiceModels;

public record PairToCheck(
    int FirstId,
    string FirstName,
    string FirstSymbol,
    int  SecondId,
    string SecondName,
    string SecondSymbol);
