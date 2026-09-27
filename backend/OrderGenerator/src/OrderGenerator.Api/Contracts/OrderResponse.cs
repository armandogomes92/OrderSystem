using OrderGenerator.Core.Orders;

namespace OrderGenerator.Api.Contracts;

public sealed record OrderResponse(
    string ClOrdId,
    string OrderId,
    OrderStatus Status,
    string Symbol,
    OrderSide Side,
    decimal Quantity,
    decimal Price,
    string? RejectReason);