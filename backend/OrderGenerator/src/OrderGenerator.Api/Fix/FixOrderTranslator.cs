using OrderGenerator.Api.Contracts;
using OrderGenerator.Core.Orders;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderGenerator.Api.Fix;

public static class FixOrderTranslator
{
    public static NewOrderSingle ToNewOrderSingle(Order order, string clOrdId) =>
        new(
            new ClOrdID(clOrdId),
            new Symbol(order.Symbol),
            new Side(order.Side switch
            {
                OrderSide.Buy => Side.BUY,
                OrderSide.Sell => Side.SELL,
                _ => throw new ArgumentOutOfRangeException(nameof(order), $"Lado não suportado: {order.Side}")
            }),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT))
        {
            OrderQty = new OrderQty(order.Quantity),
            Price = new Price(order.Price)
        };

    public static OrderResponse ToOrderResponse(ExecutionReport report) =>
        new(
            ClOrdId: report.ClOrdID.Value,
            OrderId: report.OrderID.Value,
            Status: report.ExecType.Value switch
            {
                ExecType.NEW => OrderStatus.New,
                ExecType.REJECTED => OrderStatus.Rejected,
                var other => throw new InvalidOperationException($"ExecType não esperado: {other}")
            },
            Symbol: report.Symbol.Value,
            Side: report.Side.Value switch
            {
                Side.BUY => OrderSide.Buy,
                Side.SELL => OrderSide.Sell,
                var other => throw new InvalidOperationException($"Lado não esperado: {other}")
            },
            Quantity: report.OrderQty.Value,
            Price: report.Price.Value,
            RejectReason: report.IsSetField(Tags.Text) ? report.Text.Value : null);
}