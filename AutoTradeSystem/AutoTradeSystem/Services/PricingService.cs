using AutoTradeSystem.Interfaces;
using Serilog;
using System;
using System.Globalization;
using System.Text.Json;

namespace AutoTradeSystem.Services
{
    public class PricingService : IPricingService
    {
        private readonly ILogger<PricingService> _logger;
        private IConfiguration _configuration;
        private string _baseURL;
        public PricingService(ILogger<PricingService> logger, IConfiguration configuration) 
        { 
            _logger = logger;
            _configuration = configuration;
            _baseURL = _configuration["PricingSystemBaseURL"];
        }
        public async Task<decimal> GetPriceFromTicker(string ticker)
        {
            try
            {
                HttpClient client = new HttpClient();

                _logger.LogInformation($"GetPriceFromTicker Request for Ticker {ticker}, sent at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                using (HttpResponseMessage response = await client.GetAsync(_baseURL + "/GetPrice/" + ticker).ConfigureAwait(false))
                {
                    using (HttpContent content = response.Content)
                    {
                        var json = await content.ReadAsStringAsync().ConfigureAwait(false);
                        _logger.LogInformation($"GetPriceFromTicker Response for Ticker {ticker}, received at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, response was : {json}");
                        var responseObject = JsonSerializer.Deserialize<GetPriceResponse>(json);
                        decimal? currentPrice = (responseObject?.Prices.Values.FirstOrDefault());
                        return currentPrice.HasValue ? currentPrice.Value : 0m;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetPriceFromTicker Failed for Ticker {ticker}, at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, with the following exception message" + ex.Message);
            }
            return 0m;
        }

        public async Task<IDictionary<string, decimal>> GetPrices()
        {
            try
            {
                HttpClient client = new HttpClient();
                _logger.LogInformation($"GetAllPrices Request sent at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                using (HttpResponseMessage response = await client.GetAsync(_baseURL + "/GetAllPrices").ConfigureAwait(false))
                {
                    using (HttpContent content = response.Content)
                    {
                        var json = await content.ReadAsStringAsync().ConfigureAwait(false);
                        var responseObject = JsonSerializer.Deserialize<GetPriceResponse>(json);
                        _logger.LogInformation($"GetAllPrices Response received at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, response was : {json}");
                        IDictionary<string, decimal> prices = responseObject?.Prices;
                        return prices ?? new Dictionary<string, decimal>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetAllPrices Failed at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, with the following exception message" + ex.Message);
            }
            return new Dictionary<string, decimal>();
        }
        public async Task<IList<string>> GetTickers()
        {
            try
            {
                HttpClient client = new HttpClient();

                _logger.LogInformation($"GetTickers Request sent at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");

                using (HttpResponseMessage response = await client.GetAsync(_baseURL + "/GetTickers").ConfigureAwait(false))
                {
                    using (HttpContent content = response.Content)
                    {
                        var json = await content.ReadAsStringAsync().ConfigureAwait(false);
                        _logger.LogInformation($"GetTickers Response received at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, response was : {json}");
                        var responseObject = JsonSerializer.Deserialize<GetTickersResponse>(json);
                        IList<string> tickers = responseObject?.Tickers;
                        return tickers ?? new List<string>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetTickers Failed at Time :{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, with the following exception message" + ex.Message);
            }
            return new List<string>();
        }
    }
}
