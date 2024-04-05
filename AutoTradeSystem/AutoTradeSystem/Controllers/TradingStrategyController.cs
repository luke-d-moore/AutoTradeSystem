using AutoTradeSystem.Services;
using Microsoft.AspNetCore.Mvc;
using AutoTradeSystem.Dtos;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AutoTradeSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradingStrategyController : ControllerBase
    {
        private readonly ILogger<TradingStrategyController> _logger;
        private readonly IAutoTradingStrategyService _autoTradingStrategyService;

        public TradingStrategyController(ILogger<TradingStrategyController> logger, IAutoTradingStrategyService autoTradingStrategyService)//, IHostedServiceAccessor<IAutoTradingStrategyService> autoTradingStrategyService) 
        {
            _logger = logger;
            _autoTradingStrategyService = autoTradingStrategyService;
        }

        // GET: api/<TradingStrategyController>
        [HttpGet]
        public GetStrategiesResponse Get()
        {
            return new GetStrategiesResponse(true, "Strategies Retrieved", _autoTradingStrategyService.GetStrategies());
        }

        // POST api/<TradingStrategyController>
        [HttpPost]
        [SwaggerOperation(nameof(AddStrategy))]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(Response))]
        public async Task<ActionResult<AddStrategyResponse>> AddStrategy(TradingStrategyDto tradingStrategy)
        {
            if (tradingStrategy == null) return BadRequest("Invalid Strategy");
            if (tradingStrategy.Ticker == null || (tradingStrategy.Ticker.Length > 5 || tradingStrategy.Ticker.Length < 3))
            {
                _logger.LogError("Invalid Ticker {0}", tradingStrategy.Ticker);
                return BadRequest("Invalid Ticker");
            }
            if (tradingStrategy.Quantity <= 0)
            {
                _logger.LogError("Invalid Quantity {0}", tradingStrategy.Quantity);
                return BadRequest("Invalid Quantity. Quantity must be greater than 0.");
            }
            if (tradingStrategy.PriceChange <= 0)
            {
                _logger.LogError("Invalid PriceChange {0}", tradingStrategy.PriceChange);
                return BadRequest("Invalid PriceChange. PriceChange must be greater than 0.");
            }
            var added = await _autoTradingStrategyService.AddStrategy(tradingStrategy);
            if (added)
            {
                _logger.LogInformation("Strategy Added Successfully");
            }
            else
            {
                _logger.LogError("Failed To Add Strategy {@strategyDetails}", tradingStrategy);
            }

            if (added)
            {
                return Ok(new AddStrategyResponse(true, "Strategy Added Successfully", tradingStrategy));
            }
            else
            {
                return BadRequest(new AddStrategyResponse(false, "Failed To Add Strategy", tradingStrategy));
            }
        }

        // DELETE api/<TradingStrategyController>/5
        [HttpDelete("{id}")]
        [SwaggerOperation(nameof(DeleteStrategy))]
        [SwaggerResponse(StatusCodes.Status200OK, "OK")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found")]
        public async Task<ActionResult<RemoveStrategyResponse>> DeleteStrategy(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogError("Invalid ID {0}", id);
                return BadRequest("Invalid ID");
            }
            var removed = await _autoTradingStrategyService.RemoveStrategy(id);
            if (removed)
            {
                _logger.LogInformation("Strategy Removed Successfully");
            }
            else
            {
                _logger.LogError("Failed To Remove Strategy {0}", id);
            }

            if (removed)
            {
                return Ok(new RemoveStrategyResponse(true, "Strategy Removed Successfully", id));
            }
            else
            {
                return NotFound(new RemoveStrategyResponse(false, "Strategy Not Found", id));
            }
        }
    }
}
