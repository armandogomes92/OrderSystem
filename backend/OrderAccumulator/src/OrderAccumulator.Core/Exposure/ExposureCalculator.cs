using OrderAccumulator.Core.Orders;

namespace OrderAccumulator.Core.Exposure;

public sealed class ExposureCalculator
{
    public const decimal ExposureLimit = 100_000_000m;

    private readonly Dictionary<string, decimal> _exposures = new();
    private readonly Lock _lock = new();

    public ExposureDecision Apply(Order order)
    {
        var orderValue = order.Price * order.Quantity;
        var signedValue = order.Side == OrderSide.Buy ? orderValue : -orderValue;

        lock (_lock)
        {
            var current = _exposures.GetValueOrDefault(order.Symbol);
            var projected = current + signedValue;

            if (Math.Abs(projected) > ExposureLimit)
            {
                return new ExposureDecision(
                    Accepted: false,
                    Exposure: current,
                    RejectReason: $"Limite de exposição de {ExposureLimit} excedido para {order.Symbol}.");
            }

            _exposures[order.Symbol] = projected;
            return new ExposureDecision(Accepted: true, Exposure: projected);
        }
    }

    public decimal GetExposure(string symbol)
    {
        lock (_lock)
        {
            return _exposures.GetValueOrDefault(symbol);
        }
    }
}