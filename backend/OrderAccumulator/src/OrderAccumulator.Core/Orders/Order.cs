namespace OrderAccumulator.Core.Orders;

public sealed record Order(string Symbol, OrderSide Side, decimal Quantity, decimal Price);