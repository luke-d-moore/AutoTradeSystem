using Serilog;
using System;
using System.Text.Json;

namespace AutoTradeSystem.Services
{
    public class TradeActionService : ITradeActionService
    {
        private readonly ILogger<TradeActionService> _logger;
        private IConfiguration _configuration;
        private string _baseURL;
        public TradeActionService(ILogger<TradeActionService> logger, IConfiguration configuration) 
        { 
            _logger = logger;
            _configuration = configuration;
            _baseURL = _configuration["TradeActionSystemBaseURL"];
        }
        public async Task<decimal> Buy(string ticker, int Quantity, decimal OriginalPrice)
        {
            try
            {
                HttpClient client = new HttpClient();

                using (HttpResponseMessage response = await client.GetAsync(_baseURL + "/Buy/" + ticker))
                {
                    using (HttpContent content = response.Content)
                    {
                        var json = await content.ReadAsStringAsync();
                        var responseObject = JsonSerializer.Deserialize<GetPriceResponse>(json);
                        decimal? currentPrice = (responseObject?.Prices.Values.FirstOrDefault());
                        return currentPrice.HasValue ? currentPrice.Value : 0m;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Buy Failed with the following exception message" + ex.Message);
            }
            return 0m;
        }

        public async Task<decimal> Sell(string ticker, int Quantity, decimal OriginalPrice)
        {
            try
            {
                HttpClient client = new HttpClient();

                using (HttpResponseMessage response = await client.GetAsync(_baseURL + "/Sell"))
                {
                    using (HttpContent content = response.Content)
                    {
                        var json = await content.ReadAsStringAsync();
                        var responseObject = JsonSerializer.Deserialize<GetPriceResponse>(json);
                        decimal? currentPrice = (responseObject?.Prices.Values.FirstOrDefault());
                        return currentPrice.HasValue ? currentPrice.Value : 0m;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sell Failed with the following exception message" + ex.Message);
            }
            return 0m;
        }
    }
}
