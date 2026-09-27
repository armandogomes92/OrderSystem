using System.Globalization;
using OrderGenerator.Core.Orders;
using OrderGenerator.Core.Validation;

namespace OrderGenerator.Tests.CoreValidation;

public class OrderValidatorTests
{
    private static Order ValidOrder() => new("PETR4", OrderSide.Buy, 100, 10.50m);

    private static decimal Dec(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    [Fact]
    public void Ordem_valida_nao_tem_erros()
    {
        Assert.Empty(OrderValidator.Validate(ValidOrder()));
    }

    [Theory]
    [InlineData("PETR4")]
    [InlineData("VALE3")]
    [InlineData("VIIA4")]
    public void Simbolos_permitidos_sao_aceitos(string symbol)
    {
        Assert.Empty(OrderValidator.Validate(ValidOrder() with { Symbol = symbol }));
    }

    [Theory]
    [InlineData("ITUB4")]
    [InlineData("petr4")]
    [InlineData("")]
    [InlineData(null)]
    public void Simbolo_fora_da_lista_e_rejeitado(string? symbol)
    {
        var errors = OrderValidator.Validate(ValidOrder() with { Symbol = symbol! });

        Assert.True(errors.ContainsKey("symbol"));
    }

    [Fact]
    public void Lado_invalido_e_rejeitado()
    {
        var errors = OrderValidator.Validate(ValidOrder() with { Side = (OrderSide)99 });

        Assert.True(errors.ContainsKey("side"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99_999)]
    public void Quantidade_dentro_do_limite_e_aceita(int quantity)
    {
        Assert.Empty(OrderValidator.Validate(ValidOrder() with { Quantity = quantity }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100_000)]
    public void Quantidade_fora_do_limite_e_rejeitada(int quantity)
    {
        var errors = OrderValidator.Validate(ValidOrder() with { Quantity = quantity });

        Assert.True(errors.ContainsKey("quantity"));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("10.50")]
    [InlineData("999.99")]
    public void Preco_valido_e_aceito(string price)
    {
        Assert.Empty(OrderValidator.Validate(ValidOrder() with { Price = Dec(price) }));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("1000")]
    [InlineData("1000.01")]
    [InlineData("10.005")]
    public void Preco_invalido_e_rejeitado(string price)
    {
        var errors = OrderValidator.Validate(ValidOrder() with { Price = Dec(price) });

        Assert.True(errors.ContainsKey("price"));
    }

    [Fact]
    public void Varios_erros_sao_retornados_juntos()
    {
        var errors = OrderValidator.Validate(new Order("XXXX", OrderSide.Buy, 0, 0m));

        Assert.True(errors.ContainsKey("symbol"));
        Assert.True(errors.ContainsKey("quantity"));
        Assert.True(errors.ContainsKey("price"));
    }
}