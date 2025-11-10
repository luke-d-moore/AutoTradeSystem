using Microsoft.AspNetCore.Mvc;
using AutoTradeSystem.Dtos;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using AutoTradeSystem.Interfaces;

namespace AutoTradeSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradingStrategyController : ControllerBase
    {
        private readonly ILogger<TradingStrategyController> _logger;
        private readonly IAutoTradingStrategyService _autoTradingStrategyService;

        public TradingStrategyController(ILogger<TradingStrategyController> logger, IAutoTradingStrategyService autoTradingStrategyService)
        {
            _logger = logger;
            _autoTradingStrategyService = autoTradingStrategyService;
        }

        // GET: api/<TradingStrategyController>
        [HttpGet]
        [SwaggerOperation(nameof(GetAllStrategies))]
        [SwaggerResponse(StatusCodes.Status200OK, "OK")]
        public IActionResult GetAllStrategies()
        {
            var response = new GetStrategiesResponse(true, "Strategies Retrieved", _autoTradingStrategyService.GetStrategies());
            return Ok(response);
        }

        // POST api/<TradingStrategyController>
        [HttpPost]
        [SwaggerOperation(nameof(AddStrategy))]
        [SwaggerResponse(StatusCodes.Status200OK, "OK")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Not Found")]
        public async Task<IActionResult> AddStrategy(TradingStrategyDto tradingStrategy)
        {
            if (tradingStrategy == null) return BadRequest("Invalid Strategy");
            if (tradingStrategy.Ticker == null || (tradingStrategy.Ticker.Length > 5 || tradingStrategy.Ticker.Length < 3))
            {
                _logger.LogError($"Invalid Ticker {tradingStrategy.Ticker}");
                return Problem(
                    type: "Bad Request",
                    title: "Invalid Ticker",
                    detail: $"Invalid Ticker {tradingStrategy.Ticker}",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
            if (tradingStrategy.Quantity <= 0)
            {
                _logger.LogError($"Invalid Quantity {tradingStrategy.Quantity}");
                return Problem(
                    type: "Bad Request",
                    title: "Invalid Quantity",
                    detail: $"Invalid Quantity. Quantity must be greater than 0.",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
            if (tradingStrategy.PriceChange <= 0)
            {
                _logger.LogError($"Invalid PriceChange {tradingStrategy.PriceChange}");
                return Problem(
                    type: "Bad Request",
                    title: "Invalid PriceChange",
                    detail: $"Invalid PriceChange. PriceChange must be greater than 0.",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
            var added = await _autoTradingStrategyService.AddStrategy(tradingStrategy).ConfigureAwait(false);
            if (added)
            {
                _logger.LogInformation("Strategy Added Successfully");
                return Ok(new AddStrategyResponse(true, "Strategy Added Successfully", tradingStrategy));
            }
            else
            {
                _logger.LogError("Failed To Add Strategy {@strategyDetails}", tradingStrategy);
                return Problem(
                    type: "Bad Request",
                    title: "Failed To Add Strategy",
                    detail: $"Failed To Add Strategy",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
        }

        // UPDATE api/<TradingStrategyController>/5
        [HttpPut]
        [SwaggerOperation(nameof(UpdateStrategy))]
        [SwaggerResponse(StatusCodes.Status200OK, "OK")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found")]
        public async Task<IActionResult> UpdateStrategy(string id, TradingStrategyDto tradingStrategy)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogError("Invalid ID {0}", id);
                return Problem(
                    type: "Bad Request",
                    title: "Invalid Strategy ID",
                    detail: $"Strategy Not Found with id {id}",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
            var updated = await _autoTradingStrategyService.UpdateStrategy(id, tradingStrategy).ConfigureAwait(false);
            if (updated)
            {
                _logger.LogInformation("Strategy Updated Successfully");
                return Ok(new UpdateStrategyResponse(true, "Strategy Updated Successfully", id, tradingStrategy));
            }
            else
            {
                _logger.LogError("Failed To Update Strategy {0}", id);
                return Problem(
                    type: "Bad Request",
                    title: "Failed To Update Strategy",
                    detail: $"Failed To Update Strategy with id {id}",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
        }

        // DELETE api/<TradingStrategyController>/5
        [HttpDelete("{id}")]
        [SwaggerOperation(nameof(DeleteStrategy))]
        [SwaggerResponse(StatusCodes.Status200OK, "OK")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found")]
        public async Task<IActionResult> DeleteStrategy(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogError("Invalid ID {0}", id);
                return Problem(
                    type: "Bad Request",
                    title: "Invalid Strategy ID",
                    detail: $"Strategy Not Found with id {id}",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
            var removed = await _autoTradingStrategyService.RemoveStrategy(id).ConfigureAwait(false);
            if (removed)
            {
                _logger.LogInformation("Strategy Removed Successfully");
                return Ok(new RemoveStrategyResponse(true, "Strategy Removed Successfully", id));
            }
            else
            {
                _logger.LogError("Failed To Remove Strategy {0}", id);
                return Problem(
                        type: "Bad Request",
                        title: "Failed To Remove Strategy",
                        detail: $"Strategy Not Found with id {id}",
                        statusCode: StatusCodes.Status400BadRequest
                        );
            }
        }
    }
}
