using FluentMigrator;

namespace FuturesPriceComparison.PriceChecker.Migrations;

[Migration(20250401)]
public class InitialMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        var sql = File.ReadAllText("../Migrations/init.sql");
        Execute.Sql(sql);
    }
}