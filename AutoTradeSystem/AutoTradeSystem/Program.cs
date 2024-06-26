using AutoTradeSystem.Services;
using Serilog;
using AutoTradeSystem.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
var configuration = configurationBuilder.Build();

builder.Services.AddSingleton<IConfiguration>(configuration);
LogConfiguration.ConfigureSerilog(configuration);
builder.Services.AddLogging(configure => { configure.AddSerilog(); });

builder.Services.AddSingleton<IPricingService, PricingService>();
builder.Services.AddSingleton<IAutoTradingStrategyService, AutoTradingStrategyService>();
builder.Services.AddHostedService(p => p.GetRequiredService<IAutoTradingStrategyService>());
builder.Services.AddSingleton<IHighFrequencyAutoTradingService, HighFrequencyAutoTradingService>();
builder.Services.AddHostedService(p => p.GetRequiredService<IHighFrequencyAutoTradingService>());


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
