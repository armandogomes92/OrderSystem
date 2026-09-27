using OrderGenerator.Core.Orders;

namespace OrderGenerator.Api.Contracts;

public sealed record OrderRequest(string Symbol, OrderSide Side, int Quantity, decimal Price);