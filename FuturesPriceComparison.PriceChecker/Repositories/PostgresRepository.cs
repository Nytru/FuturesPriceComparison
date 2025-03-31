using Dapper;
using Npgsql;

namespace FuturesPriceComparison.PriceChecker.Repositories;

public class PostgresRepository
{
    private readonly NpgsqlDataSource _postgresDataSource;

    public PostgresRepository(NpgsqlDataSource  postgresDataSource)
    {
        _postgresDataSource = postgresDataSource;
    }

    public async Task<IEnumerable<PairToCheck>> GetPairsToCheck(CancellationToken cancellationToken = default)
    {
        await using var connection = _postgresDataSource.CreateConnection();
        const string selectPairsCommand = """
                                          select f.id as first_id,
                                                 f.name as first_name,
                                                 f.symbol as first_symbol,
                                                 f2.id as second_id,
                                                 f2.name as second_name,
                                                 f2.symbol as second_symbol
                                          from futures_pairs_to_check p
                                          join futures f on f.id = p.first_futures
                                          join futures f2 on f2.id = p.second_futures
                                          where f.symbol != ''
                                            and f2.symbol != '';
                                          """;
        var commandDefinition = new CommandDefinition(selectPairsCommand, cancellationToken: cancellationToken);
        var result = await connection.QueryAsync<PairToCheck>(commandDefinition);
        return result;
    }

    public async Task<LastFuturesPrice> GetLastAvailablePrice(int firmId)
    {
        await using var connection = _postgresDataSource.CreateConnection();
        const string selectPairsCommand = """
                                          select f.price,
                                                 f.timestamp_utc
                                          from futures_prices f
                                          where f.futures_id = @firmId
                                          order by f.timestamp_utc desc
                                          limit 1;
                                          """;

        var commandDefinition = new CommandDefinition(selectPairsCommand, new { firmId });
        var result = await connection.QuerySingleAsync<LastFuturesPrice>(commandDefinition);
        return result;
    }

    public async Task SaveNewPrice(int futuresId, decimal price, DateTime timestamp)
    {
        await using var connection = _postgresDataSource.CreateConnection();
        const string command = """
                               insert into futures_prices(price, timestamp_utc, futures_id)
                               values (@price, @timestamp, @id);
                               """;
        var commandDefinition = new CommandDefinition(command, new { price, timestamp, id = futuresId });
        await connection.ExecuteAsync(commandDefinition);
    }

    public async Task SaveDifference(int firstFuturesId, int secondFuturesId, decimal difference, DateTime timestampUtc)
    {
        await using var connection = _postgresDataSource.CreateConnection();
        const string command = """
                               insert into price_difference(first_futures, second_futures, difference, timestamp)
                               values (@first, @second, @difference, @timestamp);
                               """;
        var commandDefinition = new CommandDefinition(command, new
        {
            first = firstFuturesId,
            second = secondFuturesId,
            difference,
            timestamp = timestampUtc
        });
        await connection.ExecuteAsync(commandDefinition);
    }
}

public record PairToCheck(
    int FirstId,
    string FirstName,
    string FirstSymbol,
    int  SecondId,
    string SecondName,
    string SecondSymbol);

public record LastFuturesPrice(decimal Price, DateTime TimestampUtc);
