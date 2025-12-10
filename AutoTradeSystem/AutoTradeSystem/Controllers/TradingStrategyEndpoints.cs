using AutoTradeSystem.Dtos;
using AutoTradeSystem.Interfaces;

namespace AutoTradeSystem.Controllers
{
    public static class TradingStrategyEndpoints
    {
        public static void MapEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("minimalapi/TradingStrategy/");
            group.MapGet(nameof(GetAllStrategies), GetAllStrategies);
            group.MapPost(nameof(AddStrategy), AddStrategy);
            group.MapPut(nameof(UpdateStrategy), UpdateStrategy);
            group.MapDelete(nameof(DeleteStrategy), DeleteStrategy);
        }

        public static async Task<IResult> GetAllStrategies(IAutoTradingStrategyService autoTradingStrategyService)
        {
            var response = new GetStrategiesResponse(true, "Strategies Retrieved", autoTradingStrategyService.GetStrategies());
            return Results.Ok(response);
        }

        public static async Task<IResult> AddStrategy(TradingStrategyDto tradingStrategy, IAutoTradingStrategyService autoTradingStrategyService)
        {
            var added = await autoTradingStrategyService.AddStrategy(tradingStrategy).ConfigureAwait(false);
            if (added)
            {
                return Results.Ok(new AddStrategyResponse(true, "Strategy Added Successfully", tradingStrategy));
            }
            else
            {
                return Results.Problem(
                    type: "Bad Request",
                    title: "Failed To Add Strategy",
                    detail: $"Failed To Add Strategy",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
        }
        public static async Task<IResult> UpdateStrategy(string id, TradingStrategyDto tradingStrategy, IAutoTradingStrategyService autoTradingStrategyService)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Results.Problem(
                    type: "Bad Request",
                    title: "Invalid Strategy ID",
                    detail: "Strategy ID must be provided.",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }

            var updated = await autoTradingStrategyService.UpdateStrategy(id, tradingStrategy).ConfigureAwait(false);
            if (updated)
            {
                return Results.Ok(new UpdateStrategyResponse(true, "Strategy Updated Successfully", id, tradingStrategy));
            }
            else
            {
                return Results.Problem(
                    type: "Not Found",
                    title: "Failed To Update Strategy",
                    detail: $"Failed To Update Strategy with id {id}",
                    statusCode: StatusCodes.Status404NotFound
                    );
            }
        }
        public static async Task<IResult> DeleteStrategy(string id, IAutoTradingStrategyService autoTradingStrategyService)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Results.Problem(
                    type: "Bad Request",
                    title: "Invalid Strategy ID",
                    detail: "Strategy ID must be provided.",
                    statusCode: StatusCodes.Status400BadRequest
                    );
            }
            var removed = await autoTradingStrategyService.RemoveStrategy(id).ConfigureAwait(false);
            if (removed)
            {
                return Results.Ok(new RemoveStrategyResponse(true, "Strategy Removed Successfully", id));
            }
            else
            {
                return Results.Problem(
                        type: "Not Found",
                        title: "Failed To Remove Strategy",
                        detail: $"Strategy Not Found with id {id}",
                        statusCode: StatusCodes.Status404NotFound
                        );
            }
        }
    }
}
