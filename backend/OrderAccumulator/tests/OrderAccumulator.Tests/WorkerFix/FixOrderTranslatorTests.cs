using OrderAccumulator.Core.Exposure;
using OrderAccumulator.Core.Orders;
using OrderAccumulator.Worker.Fix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Tests.WorkerFix;

public class FixOrderTranslatorTests
{
    private static NewOrderSingle CreateNewOrderSingle(char side = Side.BUY) 
        => new(new ClOrdID("ORD-1"), new Symbol("PETR4"), new Side(side), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT))
    {
        OrderQty = new OrderQty(100m),
        Price = new Price(10.50m)
    };

    [Theory]
    [InlineData(Side.BUY, OrderSide.Buy)]
    [InlineData(Side.SELL, OrderSide.Sell)]
    public void ToOrder_converte_os_campos_da_mensagem(char fixSide, OrderSide expectedSide)
    {
        var order = FixOrderTranslator.ToOrder(CreateNewOrderSingle(fixSide));

        Assert.Equal("PETR4", order.Symbol);
        Assert.Equal(expectedSide, order.Side);
        Assert.Equal(100m, order.Quantity);
        Assert.Equal(10.50m, order.Price);
    }

    [Fact]
    public void Ordem_aceita_gera_ExecutionReport_New()
    {
        var report = FixOrderTranslator.ToExecutionReport(
            CreateNewOrderSingle(), new ExposureDecision(Accepted: true, Exposure: 1_050m));

        Assert.Equal(ExecType.NEW, report.ExecType.Value);
        Assert.Equal(OrdStatus.NEW, report.OrdStatus.Value);
        Assert.Equal("ORD-1", report.ClOrdID.Value);
        Assert.Equal("PETR4", report.Symbol.Value);
        Assert.Equal(Side.BUY, report.Side.Value);
        Assert.Equal(100m, report.LeavesQty.Value);
        Assert.Equal(0m, report.CumQty.Value);
        Assert.Equal(0m, report.AvgPx.Value);
        Assert.False(string.IsNullOrWhiteSpace(report.OrderID.Value));
        Assert.False(string.IsNullOrWhiteSpace(report.ExecID.Value));
    }

    [Fact]
    public void Ordem_rejeitada_gera_ExecutionReport_Rejected()
    {
        var report = FixOrderTranslator.ToExecutionReport(
            CreateNewOrderSingle(),
            new ExposureDecision(Accepted: false, Exposure: 100_000_000m, RejectReason: "Limite de exposição excedido"));

        Assert.Equal(ExecType.REJECTED, report.ExecType.Value);
        Assert.Equal(OrdStatus.REJECTED, report.OrdStatus.Value);
        Assert.Equal(OrdRejReason.ORDER_EXCEEDS_LIMIT, report.OrdRejReason.Value);
        Assert.Equal("Limite de exposição excedido", report.Text.Value);
        Assert.Equal("ORD-1", report.ClOrdID.Value);
        Assert.Equal(0m, report.LeavesQty.Value);
    }
}