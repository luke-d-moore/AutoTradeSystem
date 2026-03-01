using AutoTradeSystem.Controllers;
using AutoTradeSystem.Interfaces;
using AutoTradeSystem.Logging;
using AutoTradeSystem.Services;
using PricingSystem.Protos;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

LogConfiguration.ConfigureSerilog(builder.Configuration);
builder.Services.AddLogging(configure => { configure.AddSerilog(); });

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory
    {
        HostName = builder.Configuration["ConnectionHostName"] ?? "localhost",
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(
            int.Parse(builder.Configuration["NetworkRecoveryIntervalSeconds"] ?? "10"))
    };
});

builder.Services.AddSingleton<PricingService>();
builder.Services.AddSingleton<TradeActionService>();
builder.Services.AddSingleton<AutoTradingStrategyService>();
builder.Services.AddSingleton<IPricingService>(p => p.GetRequiredService<PricingService>());
builder.Services.AddSingleton<ITradeActionService>(p => p.GetRequiredService<TradeActionService>());
builder.Services.AddSingleton<IAutoTradingStrategyService>(p => p.GetRequiredService<AutoTradingStrategyService>());
builder.Services.AddHostedService(p => p.GetRequiredService<PricingService>());
builder.Services.AddHostedService(p => p.GetRequiredService<TradeActionService>());
builder.Services.AddHostedService(p => p.GetRequiredService<AutoTradingStrategyService>());

builder.Services.AddGrpcClient<GrpcPricingService.GrpcPricingServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["PricingSystemBaseURL"]);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapEndpoints();

app.MapFallbackToFile("/index.html");

app.Run();

