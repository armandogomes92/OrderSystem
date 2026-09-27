using OrderGenerator.Api.Contracts;
using OrderGenerator.Api.Fix;
using OrderGenerator.Core.Orders;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace OrderGenerator.Tests.ApiFix;

public class FixOrderTranslatorTests
{
    private static ExecutionReport CreateReport(char execType, char ordStatus, char side = Side.BUY) =>
        new(new OrderID("ACC-1"), new ExecID("EXEC-1"), new ExecType(execType), new OrdStatus(ordStatus),
            new Symbol("PETR4"), new Side(side), new LeavesQty(100m), new CumQty(0m), new AvgPx(0m))
        {
            ClOrdID = new ClOrdID("ORD-1"),
            OrderQty = new OrderQty(100m),
            Price = new Price(10.50m)
        };

    [Theory]
    [InlineData(OrderSide.Buy, Side.BUY)]
    [InlineData(OrderSide.Sell, Side.SELL)]
    public void ToNewOrderSingle_preenche_os_campos_da_mensagem(OrderSide side, char expectedFixSide)
    {
        var before = DateTime.UtcNow;

        var message = FixOrderTranslator.ToNewOrderSingle(new Order("PETR4", side, 100, 10.50m), "ORD-1");

        Assert.Equal("ORD-1", message.ClOrdID.Value);
        Assert.Equal("PETR4", message.Symbol.Value);
        Assert.Equal(expectedFixSide, message.Side.Value);
        Assert.Equal(100m, message.OrderQty.Value);
        Assert.Equal(10.50m, message.Price.Value);
        Assert.Equal(OrdType.LIMIT, message.OrdType.Value);
        Assert.InRange(message.TransactTime.Value, before, DateTime.UtcNow);
    }

    [Theory]
    [InlineData(Side.BUY, OrderSide.Buy)]
    [InlineData(Side.SELL, OrderSide.Sell)]
    public void ExecutionReport_New_vira_resposta_New(char fixSide, OrderSide expectedSide)
    {
        var response = FixOrderTranslator.ToOrderResponse(CreateReport(ExecType.NEW, OrdStatus.NEW, fixSide));

        Assert.Equal(OrderStatus.New, response.Status);
        Assert.Equal("ORD-1", response.ClOrdId);
        Assert.Equal("ACC-1", response.OrderId);
        Assert.Equal("PETR4", response.Symbol);
        Assert.Equal(expectedSide, response.Side);
        Assert.Equal(100m, response.Quantity);
        Assert.Equal(10.50m, response.Price);
        Assert.Null(response.RejectReason);
    }

    [Fact]
    public void ExecutionReport_Rejected_vira_resposta_Rejected_com_motivo()
    {
        var report = CreateReport(ExecType.REJECTED, OrdStatus.REJECTED);
        report.OrdRejReason = new OrdRejReason(OrdRejReason.ORDER_EXCEEDS_LIMIT);
        report.Text = new Text("Limite de exposição excedido.");

        var response = FixOrderTranslator.ToOrderResponse(report);

        Assert.Equal(OrderStatus.Rejected, response.Status);
        Assert.Equal("ORD-1", response.ClOrdId);
        Assert.Equal("Limite de exposição excedido.", response.RejectReason);
    }
}