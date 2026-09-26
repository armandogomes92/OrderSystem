using OrderAccumulator.Core.Exposure;
using OrderAccumulator.Core.Orders;
using QuickFix.FIX44;

namespace OrderAccumulator.Worker.Fix;

public static class FixOrderTranslator
{
    public static Order ToOrder(NewOrderSingle message) => throw new NotImplementedException();

    public static ExecutionReport ToExecutionReport(NewOrderSingle message, ExposureDecision decision)
        => throw new NotImplementedException();
}