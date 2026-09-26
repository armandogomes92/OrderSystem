using OrderAccumulator.Core.Orders;

namespace OrderAccumulator.Core.Exposure;

public sealed class ExposureCalculator
{
    public const decimal ExposureLimit = 100_000_000m;

    public ExposureDecision Apply(Order order) => throw new NotImplementedException();

    public decimal GetExposure(string symbol) => throw new NotImplementedException();
}