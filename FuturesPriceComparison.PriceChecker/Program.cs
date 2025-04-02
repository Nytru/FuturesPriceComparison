using Dapper;
using FuturesPriceComparison.PriceChecker;
using FuturesPriceComparison.PriceChecker.Binance.Jobs;
using FuturesPriceComparison.PriceChecker.Binance.Repository;
using FuturesPriceComparison.PriceChecker.Binance.Services;
using FuturesPriceComparison.PriceChecker.Constants;
using FuturesPriceComparison.PriceChecker.Exceptions;
using FuturesPriceComparison.PriceChecker.Repositories;
using FuturesPriceComparison.PriceChecker.Utilities;
using Microsoft.Extensions.Options;
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
builder.Configuration.AddAppSettings();

var services = builder.Services;

builder.Configuration.AddEnvironmentVariables();

services
    .ConfigureByName<BinanceApiOptions>(builder.Configuration)
    .AddHttpClient<IExchangeClient, BinanceClient>()
    .AddTransientHttpErrorPolicy(policy =>
    {
        return policy
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    });

services.AddNpgsql(builder.Configuration);
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.AddScoped<PostgresRepository>();
services.AddScoped<BinanceRepository>();
services.AddResiliencePipeline(PoliciesNames.DbPolicy, b =>
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
services.ConfigureByName<ScheduleOptions>(builder.Configuration);

services.AddQuartz(q =>
{
    var interval = builder.Configuration
        .GetSection(nameof(ScheduleOptions))
        .GetValue<int?>(nameof(ScheduleOptions.IntervalInSeconds));
    if (interval is null)
        throw new MissingConfigException("Section SampleOptions is missing");

    q.ScheduleJob<PriceCheckerJob>(configurator => configurator
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(interval.Value)
            .RepeatForever()));
});

services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseHttpsRedirection();

await app.RunAsync();
