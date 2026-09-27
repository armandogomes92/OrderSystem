using OrderGenerator.Api.Contracts;
using OrderGenerator.Core.Orders;
using QuickFix.FIX44;

namespace OrderGenerator.Api.Fix;

public static class FixOrderTranslator
{
    public static NewOrderSingle ToNewOrderSingle(Order order, string clOrdId) => throw new NotImplementedException();

    public static OrderResponse ToOrderResponse(ExecutionReport report) => throw new NotImplementedException();
}