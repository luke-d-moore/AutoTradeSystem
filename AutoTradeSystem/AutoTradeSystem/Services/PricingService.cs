using AutoTradeSystem.Interfaces;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace AutoTradeSystem.Services
{
    public class PricingService : PricingServiceBase, IPricingService
    {
        private readonly ILogger<PricingService> _logger;
        private IConfiguration _configuration;
        private string _baseURL;
        private IHttpClientFactory _httpClientFactory;
        private HttpClient _client;
        private const int _checkRate = 5000;
        private ConcurrentDictionary<string, decimal> _prices = new ConcurrentDictionary<string, decimal>();
        public ConcurrentDictionary<string, decimal> Prices
        {
            get { return _prices; }
            set 
            { 
                _prices = value; 
            }
        }
        public string BaseURL
        {
            get { return _baseURL; }
        }

        public PricingService(ILogger<PricingService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
            : base(logger, _checkRate)
        { 
            _logger = logger;
            _configuration = configuration;
            _baseURL = _configuration["PricingSystemBaseURL"];
            _httpClientFactory = httpClientFactory;
            _client = _httpClientFactory.CreateClient();

        }
        public async Task<IDictionary<string, decimal>> GetPrices()
        {
            var requestUrl = $"{BaseURL}/GetAllPrices";

            _logger.LogInformation($"GetAllPrices Request sent to {requestUrl}");

            try
            {
                using (HttpResponseMessage response = await _client.GetAsync(requestUrl).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    _logger.LogInformation($"GetAllPrices Response received. Response : {json}");

                    var responseObject = JsonSerializer.Deserialize<GetPriceResponse>(json);

                    IDictionary<string, decimal> prices = responseObject?.Prices;

                    return prices ?? new Dictionary<string, decimal>();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"GetAllPrices Failed due to HTTP request error. Status Code: {ex.StatusCode}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "GetAllPrices Failed JSON deserialization");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting all prices.");
            }
            return new Dictionary<string, decimal>();
        }

        private Task SetPrices(IDictionary<string, decimal> prices)
        {
            foreach (var price in prices)
            {
                Prices[price.Key] = price.Value;
            }

            return Task.CompletedTask;
        }
        protected override async Task UpdatePrices(CancellationToken cancellationToken)
        {
            var prices = await GetPrices().ConfigureAwait(false);

            await SetPrices(prices).ConfigureAwait(false);
        }

        public IDictionary<string, decimal> GetLatestPrices()
        {
            return Prices;
        }
        public decimal GetLatestPriceFromTicker(string Ticker)
        {
            return Prices[Ticker];
        }
        public IList<string> GetLatestTickers()
        {
            return Prices.Keys.ToList();
        }
    }
}
