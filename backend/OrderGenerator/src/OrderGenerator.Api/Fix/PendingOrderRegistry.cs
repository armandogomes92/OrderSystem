using System.Collections.Concurrent;
using OrderGenerator.Api.Contracts;

namespace OrderGenerator.Api.Fix;

public sealed class PendingOrderRegistry
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<OrderResponse>> _pending = new();

    public Task<OrderResponse> Register(string clOrdId)
    {
        var completion = new TaskCompletionSource<OrderResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pending.TryAdd(clOrdId, completion))
            throw new InvalidOperationException($"Já existe uma ordem aguardando resposta com ClOrdID {clOrdId}.");

        return completion.Task;
    }

    public bool TryComplete(string clOrdId, OrderResponse response) =>
        _pending.TryRemove(clOrdId, out var completion) && completion.TrySetResult(response);

    public void Remove(string clOrdId) => _pending.TryRemove(clOrdId, out _);
}