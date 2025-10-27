using AutoTradeSystem.Response;
using Serilog;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AutoTradeSystem.Classes
{
    public static class PriceChecker
    {
        private const string baseURL = "https://finnhub.io/api/v1/quote?symbol=[Ticker]&token=[Token]";
        public static async Task<decimal> GetPriceFromTicker(string ticker, string token)
        {
            try
            {
                HttpClient client = new HttpClient();

                using (HttpResponseMessage response = await client.GetAsync(baseURL.Replace("[Ticker]", ticker).Replace("[Token]", token)))
                {
                    using (HttpContent content = response.Content)
                    {
                        var json = await content.ReadAsStringAsync();
                        var responseObject = JsonSerializer.Deserialize<PriceCheckResponse>(json);
                        decimal? currentPrice = (responseObject?.currentPrice);
                        return currentPrice.HasValue? currentPrice.Value : 0m;
                    }
                }
            }catch (Exception ex)
            {
                Log.Logger.Error(ex, "GetPriceFromTicker Failed with the following exception message" + ex.Message);
            }
            return 0m;
        }
    }
}
