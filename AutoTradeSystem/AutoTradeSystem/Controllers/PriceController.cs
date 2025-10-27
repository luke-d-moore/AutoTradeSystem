using AutoTradeSystem.Services;
using Microsoft.AspNetCore.Mvc;
using AutoTradeSystem.Dtos;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using System.Net;

namespace AutoTradeSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceController : ControllerBase
    {
        private readonly ILogger<PriceController> _logger;
        private readonly IPricingService _pricingService;

        public PriceController(ILogger<PriceController> logger, IPricingService pricingService)
        {
            _logger = logger;
            _pricingService = pricingService;
        }

        // GET: api/<PriceController>
        [HttpGet("{ticker}")]
        [SwaggerOperation(nameof(Get))]
        [SwaggerResponse(StatusCodes.Status200OK, "OK")]
        public async Task<IActionResult> Get(string ticker)
        {
            try
            {
                var price = await _pricingService.GetCurrentPrice(ticker);
                var response = new GetPriceResponse(true, "Price Retrieved", new Dictionary<string, decimal>() { { ticker, price } });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Price Retrieve Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound
                );
            }
        }

        // GET: api/<PriceController>
        [HttpGet]
        [SwaggerOperation(nameof(Get))]
        [SwaggerResponse(StatusCodes.Status200OK, "OK")]
        public IActionResult GetAllPrices()
        {
            var tickers = _pricingService.GetTickers();
            var response = new GetPriceResponse(true, "Prices Retrieved", tickers);
            return Ok(response);
        }
    }
}
