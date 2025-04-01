using Dapper;
using FuturesPriceComparison.PriceChecker.Binance.Jobs;
using FuturesPriceComparison.PriceChecker.Binance.Repository;
using FuturesPriceComparison.PriceChecker.Binance.Services;
using FuturesPriceComparison.PriceChecker.Constants;
using FuturesPriceComparison.PriceChecker.Extensions;
using FuturesPriceComparison.PriceChecker.Interfaces;
using FuturesPriceComparison.PriceChecker.Repositories;
using FuturesPriceComparison.PriceChecker.Services;
using Polly;
using Polly.Retry;
using Quartz;
using Serilog;

DefaultTypeMap.MatchNamesWithUnderscores = true;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("log/log.log")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
// Add service defaults & Aspire client integrations.
// builder.AddServiceDefaults();
builder.Configuration.AddAppSettings();

var services = builder.Services;

services
    .ConfigureByName<BinanceApiOptions>(builder.Configuration)
    .AddHttpClient<IExchangeClient, BinanceClient>()
    .AddTransientHttpErrorPolicy(policy =>
    {
        return policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    });

services.AddNpgsqlDataSource("Host=localhost;Port=5432;Database=futures;Username=postgres;Password=postgres;");
services.AddSingleton<IDateTmeProvider, DateTmeProvider>();
services.AddScoped<PostgresRepository>();
services.AddScoped<BinancePostgresRepository>();
services.AddResiliencePipeline(PoliciesNames.PostgresPolicy, b =>
{
    b.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder()
            .Handle<Exception>(),
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 3
    });
});
services.AddScoped<PriceCheckerJob>();
services.AddProblemDetails();
services.AddOpenApi();
services.AddQuartz(q =>
{
    q.ScheduleJob<PriceCheckerJob>(configurator => configurator
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(1)
            .RepeatForever()));
});
services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await app.RunAsync();
