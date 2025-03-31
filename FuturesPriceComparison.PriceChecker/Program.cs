using Dapper;
using FuturesPriceComparison.PriceChecker.Extensions;
using FuturesPriceComparison.PriceChecker.Interfaces;
using FuturesPriceComparison.PriceChecker.Jobs;
using FuturesPriceComparison.PriceChecker.Repositories;
using FuturesPriceComparison.PriceChecker.Services;
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
    .AddHttpClient<IExchangeClient, BinanceClient>();

services.AddNpgsqlDataSource("Host=localhost;Port=5432;Database=futures;Username=postgres;Password=postgres;");
services.AddScoped<PostgresRepository>();

services.AddScoped<PriceCheckerJob>();
services.AddScoped<PriceCheckerService>();

services.AddProblemDetails();
services.AddControllers();

services.AddOpenApi();
services.AddQuartz(q =>
{
    // q.ScheduleJob<ConsoleWriterJob>(configurator => configurator
    //     .WithSimpleSchedule(x => x
    //         .WithIntervalInSeconds(3)
    //         .RepeatForever()));

    // var priceCheckerJobKey =  new JobKey("PriceCheckerJob");
    // q.AddJob<PriceCheckerJob>(opts => opts.WithIdentity(priceCheckerJobKey));
    // q.AddTrigger(opts => opts
    //     .ForJob(priceCheckerJobKey)
    //     .WithIdentity("PriceCheckerJob")
    //     .WithSimpleSchedule(x => x
    //         .WithIntervalInHours(1)
    //         .RepeatForever()));

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
app.MapControllers();

await app.RunAsync();
