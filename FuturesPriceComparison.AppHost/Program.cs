var builder = DistributedApplication.CreateBuilder(args);

var inventoryDatabase = builder
    .AddPostgres("my-postgres")
    .AddDatabase("futures-price");
builder.AddProject<Projects.FuturesPriceComparison_PriceChecker>("PriceChecker")
    .WithReference(inventoryDatabase);

await builder.Build().RunAsync();
