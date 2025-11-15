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
        private IHttpClientFactory _httpClientFactory;
        private HttpClient _client;
        public string BaseURL
        {
            get { return _baseURL; }
        }

        public PricingService(ILogger<PricingService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory) 
        { 
            _logger = logger;
            _configuration = configuration;
            _baseURL = _configuration["PricingSystemBaseURL"];
            _httpClientFactory = httpClientFactory;
            _client = _httpClientFactory.CreateClient();

        }
        public async Task<decimal> GetPriceFromTicker(string ticker)
        {
            if (string.IsNullOrEmpty(ticker))
            {
                _logger.LogWarning("GetPriceFromTicker called with null or empty ticker.");
                throw new ArgumentException("Ticker cannot be null or empty.", nameof(ticker));
            }

            var requestUrl = $"{BaseURL}/GetPrice/{ticker}";
            _logger.LogInformation($"GetPriceFromTicker Request sent to {requestUrl}");

            try
            {
                using (HttpResponseMessage response = await _client.GetAsync(requestUrl).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _logger.LogInformation($"GetPriceFromTicker Response received for Ticker {ticker}, response : {json}");

                    var responseObject = JsonSerializer.Deserialize<GetPriceResponse>(json);

                    decimal? currentPrice = (responseObject?.Prices?.Values.FirstOrDefault());

                    if (!currentPrice.HasValue || currentPrice.Value <= 0m)
                    {
                        throw new InvalidOperationException($"Invalid or missing price data in valid response for ticker: {ticker}");
                    }

                    return currentPrice.Value;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP request failed for Ticker {ticker}. Status Code: {ex.StatusCode}");
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching price for Ticker {ticker}.");
            }
            return 0;
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

        public async Task<IList<string>> GetTickers()
        {
            var requestUrl = $"{BaseURL}/GetTickers";
            _logger.LogInformation($"GetTickers Request sent to {requestUrl}");

            try
            {
                using (HttpResponseMessage response = await _client.GetAsync(requestUrl).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _logger.LogInformation($"GetTickers Response received. Response : {json}");

                    var responseObject = JsonSerializer.Deserialize<GetTickersResponse>(json);

                    IList<string> tickers = responseObject?.Tickers;

                    return tickers ?? new List<string>();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"GetTickers Failed due to HTTP request error. Status Code: {ex.StatusCode}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "GetTickers Failed JSON deserialization");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting tickers.");
            }
            return new List<string>();
        }
    }
}
