using Dapper;
using FuturesPriceComparison.Models.ServiceModels;
using Npgsql;

namespace FuturesPriceComparison.PriceChecker.Repositories;

public class PostgresRepository(NpgsqlConnection connection)
{
    public async Task<NpgsqlTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        await connection.OpenAsync(cancellationToken);
        return await connection.BeginTransactionAsync(cancellationToken);
    }

    public async Task<IEnumerable<PairToCheck>> GetPairsToCheck(
        string exchangeName,
        CancellationToken cancellationToken = default)
    {
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
                                          join exchanges e1 on e1.id = f.exchange_id
                                          join exchanges e2 on e2.id = f2.exchange_id
                                          where f.symbol != '' and e1.name = @exchange_name
                                            and f2.symbol != '' and e2.name = @exchange_name;
                                          """;
        var commandDefinition = new CommandDefinition(
            selectPairsCommand,
            new {exchange_name = exchangeName},
            cancellationToken: cancellationToken);
        var result = await connection.QueryAsync<PairToCheck>(commandDefinition);
        return result;
    }

    public async Task<LastFuturesPrice> GetLastAvailablePrice(int firmId, CancellationToken cancellationToken = default)
    {
        const string selectPairsCommand = """
                                          select f.price,
                                                 f.timestamp_utc
                                          from futures_prices f
                                          where f.futures_id = @firmId
                                          order by f.timestamp_utc desc
                                          limit 1;
                                          """;

        var commandDefinition = new CommandDefinition(
            selectPairsCommand,
            new { firmId },
            cancellationToken: cancellationToken);
        var result = await connection.QuerySingleAsync<LastFuturesPrice>(commandDefinition);
        return result;
    }

    public async Task SaveNewPrice(
        int futuresId,
        decimal price,
        DateTime timestamp,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string command = """
                               insert into futures_prices(price, timestamp_utc, futures_id)
                               values (@price, @timestamp, @id);
                               """;
        var param = new { price, timestamp, id = futuresId };
        var commandDefinition = new CommandDefinition(
            command,
            param,
            transaction: transaction,
            cancellationToken: cancellationToken);
        await connection.ExecuteAsync(commandDefinition);
    }

    public async Task SaveDifference(
        int firstFuturesId,
        int secondFuturesId,
        decimal difference,
        DateTime timestampUtc,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string command = """
                               insert into price_difference(first_futures, second_futures, difference, timestamp)
                               values (@first, @second, @difference, @timestamp);
                               """;
        var param = new
        {
            first = firstFuturesId,
            second = secondFuturesId,
            difference,
            timestamp = timestampUtc
        };
        var commandDefinition = new CommandDefinition(
            command,
            param,
            transaction: transaction,
            cancellationToken: cancellationToken);
        await connection.ExecuteAsync(commandDefinition);
    }
}
