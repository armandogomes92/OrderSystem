using OrderGenerator.Api.Contracts;

namespace OrderGenerator.Api.Fix;

public sealed class PendingOrderRegistry
{
    public Task<OrderResponse> Register(string clOrdId) => throw new NotImplementedException();

    public bool TryComplete(string clOrdId, OrderResponse response) => throw new NotImplementedException();

    public void Remove(string clOrdId) => throw new NotImplementedException();
}