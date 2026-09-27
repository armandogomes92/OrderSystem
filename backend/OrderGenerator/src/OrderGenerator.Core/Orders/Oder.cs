namespace OrderGenerator.Core.Orders;

public sealed record Order(string Symbol, OrderSide Side, int Quantity, decimal Price);