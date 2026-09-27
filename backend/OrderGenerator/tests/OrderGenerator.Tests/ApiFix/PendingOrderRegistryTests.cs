using OrderGenerator.Api.Contracts;
using OrderGenerator.Api.Fix;
using OrderGenerator.Core.Orders;

namespace OrderGenerator.Tests.ApiFix;

public class PendingOrderRegistryTests
{
    private readonly PendingOrderRegistry _registry = new();

    private static OrderResponse Response(string clOrdId) =>
        new(clOrdId, "ACC-1", OrderStatus.New, "PETR4", OrderSide.Buy, 100m, 10.50m, null);

    [Fact]
    public void Ordem_registrada_fica_aguardando_resposta()
    {
        var pending = _registry.Register("ORD-1");

        Assert.False(pending.IsCompleted);
    }

    [Fact]
    public async Task Resposta_completa_a_ordem_aguardando()
    {
        var pending = _registry.Register("ORD-1");
        var response = Response("ORD-1");

        Assert.True(_registry.TryComplete("ORD-1", response));
        Assert.Same(response, await pending);
    }

    [Fact]
    public void Resposta_para_ClOrdID_desconhecido_e_ignorada()
    {
        Assert.False(_registry.TryComplete("ORD-X", Response("ORD-X")));
    }

    [Fact]
    public void Resposta_duplicada_e_ignorada()
    {
        _registry.Register("ORD-1");

        Assert.True(_registry.TryComplete("ORD-1", Response("ORD-1")));
        Assert.False(_registry.TryComplete("ORD-1", Response("ORD-1")));
    }

    [Fact]
    public void Ordem_removida_nao_recebe_resposta()
    {
        _registry.Register("ORD-1");
        _registry.Remove("ORD-1");

        Assert.False(_registry.TryComplete("ORD-1", Response("ORD-1")));
    }

    [Fact]
    public void ClOrdID_duplicado_nao_pode_ser_registrado()
    {
        _registry.Register("ORD-1");

        var exception = Record.Exception(() => { _ = _registry.Register("ORD-1"); });

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Respostas_sao_entregues_a_ordem_correta()
    {
        var first = _registry.Register("ORD-1");
        var second = _registry.Register("ORD-2");

        _registry.TryComplete("ORD-2", Response("ORD-2"));

        Assert.False(first.IsCompleted);
        Assert.True(second.IsCompleted);
    }
}