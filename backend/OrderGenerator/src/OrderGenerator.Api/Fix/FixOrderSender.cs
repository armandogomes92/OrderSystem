using OrderGenerator.Api.Contracts;
using OrderGenerator.Core.Orders;
using QuickFix;

namespace OrderGenerator.Api.Fix;

public sealed class FixOrderSender(GeneratorFixApp app, PendingOrderRegistry registry) : IOrderSender
{
    private static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(5);

    public async Task<OrderResponse> SendAsync(Order order, CancellationToken cancellationToken)
    {
        var sessionId = app.ActiveSession ?? throw new FixSessionUnavailableException();
        var clOrdId = Guid.NewGuid().ToString();
        var pending = registry.Register(clOrdId);

        try
        {
            if (!Session.SendToTarget(FixOrderTranslator.ToNewOrderSingle(order, clOrdId), sessionId))
                throw new FixSessionUnavailableException();

            return await pending.WaitAsync(ResponseTimeout, cancellationToken);
        }
        finally
        {
            registry.Remove(clOrdId);
        }
    }
}