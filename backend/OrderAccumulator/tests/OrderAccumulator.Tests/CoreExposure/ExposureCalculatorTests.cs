using OrderAccumulator.Core.Exposure;
using OrderAccumulator.Core.Orders;
using System.Collections.Concurrent;

namespace OrderAccumulator.Tests.CoreExposure;

public class ExposureCalculatorTests
{
    private readonly ExposureCalculator _calculator = new();

    private static Order Buy(string symbol, decimal quantity, decimal price) => new(symbol, OrderSide.Buy, quantity, price);
    private static Order Sell(string symbol, decimal quantity, decimal price) => new(symbol, OrderSide.Sell, quantity, price);

    // 80.000 × 625,00 = 50.000.000 → duas ordens atingem exatamente o limite
    private void FillToLimit(string symbol, Func<string, decimal, decimal, Order> side)
    {
        _calculator.Apply(side(symbol, 80_000, 625m));
        _calculator.Apply(side(symbol, 80_000, 625m));
    }

    [Fact]
    public void Simbolo_sem_ordens_tem_exposicao_zero()
    {
        Assert.Equal(0m, _calculator.GetExposure("PETR4"));
    }

    [Fact]
    public void Compra_aceita_aumenta_a_exposicao()
    {
        var decision = _calculator.Apply(Buy("PETR4", 100, 10.50m));

        Assert.True(decision.Accepted);
        Assert.Equal(1_050m, _calculator.GetExposure("PETR4"));
    }

    [Fact]
    public void Venda_aceita_diminui_a_exposicao()
    {
        var decision = _calculator.Apply(Sell("PETR4", 100, 10.50m));

        Assert.True(decision.Accepted);
        Assert.Equal(-1_050m, _calculator.GetExposure("PETR4"));
    }

    [Fact]
    public void Compra_e_venda_se_compensam()
    {
        _calculator.Apply(Buy("PETR4", 100, 10m));   // +1.000
        _calculator.Apply(Sell("PETR4", 40, 12.5m)); //   -500

        Assert.Equal(500m, _calculator.GetExposure("PETR4"));
    }

    [Fact]
    public void Exposicao_e_calculada_por_simbolo()
    {
        _calculator.Apply(Buy("PETR4", 100, 10m));
        _calculator.Apply(Sell("VALE3", 10, 50m));

        Assert.Equal(1_000m, _calculator.GetExposure("PETR4"));
        Assert.Equal(-500m, _calculator.GetExposure("VALE3"));
        Assert.Equal(0m, _calculator.GetExposure("VIIA4"));
    }

    [Fact]
    public void Ordem_que_atinge_exatamente_o_limite_e_aceita()
    {
        _calculator.Apply(Buy("PETR4", 80_000, 625m));
        var decision = _calculator.Apply(Buy("PETR4", 80_000, 625m));

        Assert.True(decision.Accepted);
        Assert.Equal(ExposureCalculator.ExposureLimit, _calculator.GetExposure("PETR4"));
    }

    [Fact]
    public void Compra_que_ultrapassa_o_limite_e_rejeitada()
    {
        FillToLimit("PETR4", Buy);

        var decision = _calculator.Apply(Buy("PETR4", 1, 0.01m));

        Assert.False(decision.Accepted);
        Assert.False(string.IsNullOrWhiteSpace(decision.RejectReason));
    }

    [Fact]
    public void Venda_que_ultrapassa_o_limite_negativo_e_rejeitada()
    {
        FillToLimit("PETR4", Sell);

        var decision = _calculator.Apply(Sell("PETR4", 1, 0.01m));

        Assert.False(decision.Accepted);
    }

    [Fact]
    public void Ordem_rejeitada_nao_altera_a_exposicao()
    {
        FillToLimit("PETR4", Buy);

        var decision = _calculator.Apply(Buy("PETR4", 1, 0.01m));

        Assert.False(decision.Accepted);
        Assert.Equal(ExposureCalculator.ExposureLimit, decision.Exposure);
        Assert.Equal(ExposureCalculator.ExposureLimit, _calculator.GetExposure("PETR4"));
    }

    [Fact]
    public void Ordem_que_reduz_a_exposicao_e_aceita_mesmo_no_limite()
    {
        FillToLimit("PETR4", Buy);

        var decision = _calculator.Apply(Sell("PETR4", 1, 0.01m));

        Assert.True(decision.Accepted);
        Assert.Equal(99_999_999.99m, _calculator.GetExposure("PETR4"));
    }

    [Fact]
    public void Ordens_concorrentes_nunca_ultrapassam_o_limite()
    {
        var decisions = new ConcurrentBag<ExposureDecision>();

        Parallel.For(0, 1_000, _ =>
            decisions.Add(_calculator.Apply(Buy("PETR4", 80_000, 625m))));

        Assert.Equal(2, decisions.Count(d => d.Accepted));
        Assert.Equal(ExposureCalculator.ExposureLimit, _calculator.GetExposure("PETR4"));
    }
}