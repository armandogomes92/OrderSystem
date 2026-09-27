using Microsoft.AspNetCore.Http.HttpResults;
using OrderGenerator.Api.Contracts;
using OrderGenerator.Api.Fix;
using OrderGenerator.Core.Orders;
using OrderGenerator.Core.Validation;

namespace OrderGenerator.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", CreateOrder).WithName("CreateOrder");
        return app;
    }

    private static async Task<Results<Ok<OrderResponse>, ValidationProblem, ProblemHttpResult>> CreateOrder(
        OrderRequest request, IOrderSender sender, CancellationToken cancellationToken)
    {
        var order = new Order(request.Symbol, request.Side, request.Quantity, request.Price);

        var errors = OrderValidator.Validate(order);
        if (errors.Count > 0)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>(errors));

        try
        {
            return TypedResults.Ok(await sender.SendAsync(order, cancellationToken));
        }
        catch (FixSessionUnavailableException ex)
        {
            return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (TimeoutException)
        {
            return TypedResults.Problem("O OrderAccumulator não respondeu a tempo.", statusCode: StatusCodes.Status504GatewayTimeout);
        }
    }
}