using OrderAccumulator.Core.Exposure;
using OrderAccumulator.Core.Orders;
using OrderAccumulator.Worker.Fix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderAccumulator.Tests.WorkerFix;

public class FixOrderTranslatorTests
{
    private static NewOrderSingle CreateNewOrderSingle(char side = Side.BUY, decimal quantity = 100m, decimal price = 10.50m)
        => new(new ClOrdID("ORD-1"), new Symbol("PETR4"), new Side(side), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT))
        {
            OrderQty = new OrderQty(quantity),
            Price = new Price(price)
        };

    [Theory]
    [InlineData(Side.BUY, OrderSide.Buy)]
    [InlineData(Side.SELL, OrderSide.Sell)]
    public void TryToOrder_converte_os_campos_da_mensagem(char fixSide, OrderSide expectedSide)
    {
        var ok = FixOrderTranslator.TryToOrder(CreateNewOrderSingle(fixSide), out var order, out var rejectReason);

        Assert.True(ok);
        Assert.Null(rejectReason);
        Assert.NotNull(order);
        Assert.Equal("PETR4", order.Symbol);
        Assert.Equal(expectedSide, order.Side);
        Assert.Equal(100m, order.Quantity);
        Assert.Equal(10.50m, order.Price);
    }

    [Fact]
    public void TryToOrder_rejeita_lado_nao_suportado()
    {
        var ok = FixOrderTranslator.TryToOrder(CreateNewOrderSingle(Side.SELL_SHORT), out var order, out var rejectReason);

        Assert.False(ok);
        Assert.Null(order);
        Assert.False(string.IsNullOrWhiteSpace(rejectReason));
    }

    [Theory]
    [InlineData(0, 10.5)]
    [InlineData(-1, 10.5)]
    [InlineData(100, 0)]
    [InlineData(100, -1)]
    public void TryToOrder_rejeita_quantidade_ou_preco_nao_positivo(double quantity, double price)
    {
        var ok = FixOrderTranslator.TryToOrder(
            CreateNewOrderSingle(quantity: (decimal)quantity, price: (decimal)price), out var order, out var rejectReason);

        Assert.False(ok);
        Assert.Null(order);
        Assert.False(string.IsNullOrWhiteSpace(rejectReason));
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

    [Fact]
    public void Ordem_invalida_gera_ExecutionReport_Rejected()
    {
        var report = FixOrderTranslator.ToInvalidOrderReport(CreateNewOrderSingle(Side.SELL_SHORT), "Lado não suportado.");

        Assert.Equal(ExecType.REJECTED, report.ExecType.Value);
        Assert.Equal(OrdStatus.REJECTED, report.OrdStatus.Value);
        Assert.Equal(OrdRejReason.OTHER, report.OrdRejReason.Value);
        Assert.Equal("Lado não suportado.", report.Text.Value);
        Assert.Equal("ORD-1", report.ClOrdID.Value);
        Assert.Equal(0m, report.LeavesQty.Value);
    }
}
