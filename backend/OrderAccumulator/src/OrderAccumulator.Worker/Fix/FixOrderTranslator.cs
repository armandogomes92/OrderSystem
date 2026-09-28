using System.Diagnostics.CodeAnalysis;
using OrderAccumulator.Core.Exposure;
using OrderAccumulator.Core.Orders;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Worker.Fix;

public static class FixOrderTranslator
{
    public static bool TryToOrder(
        NewOrderSingle message,
        [NotNullWhen(true)] out Order? order,
        [NotNullWhen(false)] out string? rejectReason)
    {
        OrderSide? side = message.Side.Value switch
        {
            Side.BUY => OrderSide.Buy,
            Side.SELL => OrderSide.Sell,
            _ => null
        };

        rejectReason = side is null ? $"Lado não suportado: {message.Side.Value}."
            : message.OrderQty.Value <= 0 ? "A quantidade deve ser maior que zero."
            : message.Price.Value <= 0 ? "O preço deve ser maior que zero."
            : null;

        if (rejectReason is not null)
        {
            order = null;
            return false;
        }

        order = new Order(message.Symbol.Value, side!.Value, message.OrderQty.Value, message.Price.Value);
        return true;
    }

    public static ExecutionReport ToExecutionReport(NewOrderSingle message, ExposureDecision decision) =>
        decision.Accepted
            ? CreateReport(message, ExecType.NEW, OrdStatus.NEW, leavesQty: message.OrderQty.Value)
            : CreateRejectedReport(message, OrdRejReason.ORDER_EXCEEDS_LIMIT, decision.RejectReason ?? "Limite de exposição excedido.");

    public static ExecutionReport ToInvalidOrderReport(NewOrderSingle message, string reason) =>
        CreateRejectedReport(message, OrdRejReason.OTHER, reason);

    private static ExecutionReport CreateRejectedReport(NewOrderSingle message, int ordRejReason, string text)
    {
        var report = CreateReport(message, ExecType.REJECTED, OrdStatus.REJECTED, leavesQty: 0m);
        report.OrdRejReason = new OrdRejReason(ordRejReason);
        report.Text = new Text(text);
        return report;
    }

    private static ExecutionReport CreateReport(NewOrderSingle message, char execType, char ordStatus, decimal leavesQty) =>
        new(
            new OrderID(Guid.NewGuid().ToString()),
            new ExecID(Guid.NewGuid().ToString()),
            new ExecType(execType),
            new OrdStatus(ordStatus),
            new Symbol(message.Symbol.Value),
            new Side(message.Side.Value),
            new LeavesQty(leavesQty),
            new CumQty(0m),
            new AvgPx(0m))
        {
            ClOrdID = new ClOrdID(message.ClOrdID.Value),
            OrderQty = new OrderQty(message.OrderQty.Value),
            Price = new Price(message.Price.Value)
        };
}
