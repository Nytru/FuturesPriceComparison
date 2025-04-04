using FluentMigrator;

namespace FuturesPriceComparison.PriceChecker.Migrations;

[Migration(20250403)]
public class FillUpMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        const string sql = """
                           insert into exchanges (id, name)
                           values (1, 'binance');
                           
                           insert into futures (id, exchange_id, name, symbol, pair)
                           values (1, 1, 'BTCUSDT-Q', 'BTCUSDT_250627', 'BTCUSDT'),
                                  (2, 1, 'BTCUSDT-BI-Q', 'BTCUSDT_250926', 'BTCUSDT');
                           
                           insert into futures_pairs_to_check(first_futures, second_futures)
                           values(1, 2);
                           """;
        Execute.Sql(sql);
    }
}
