using Npgsql;

namespace FuturesPriceComparison.PriceChecker.BackgroundServices;

public class DateBaseInit : IHostedService
{
    private readonly NpgsqlDataSource _dataSource;

    public DateBaseInit(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var command = _dataSource.CreateCommand("create database futures-price");
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}