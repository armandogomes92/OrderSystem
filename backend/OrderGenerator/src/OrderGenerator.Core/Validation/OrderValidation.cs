using OrderGenerator.Core.Orders;

namespace OrderGenerator.Core.Validation;

public static class OrderValidator
{
    public static readonly IReadOnlySet<string> AllowedSymbols = new HashSet<string> { "PETR4", "VALE3", "VIIA4" };
    public const int MaxQuantityExclusive = 100_000;
    public const decimal MaxPriceExclusive = 1_000m;
    public const decimal PriceTick = 0.01m;

    public static IReadOnlyDictionary<string, string[]> Validate(Order order)
    {
        var errors = new Dictionary<string, string[]>();

        if (order.Symbol is null || !AllowedSymbols.Contains(order.Symbol))
            errors["symbol"] = [$"O símbolo deve ser um de: {string.Join(", ", AllowedSymbols)}."];

        if (!Enum.IsDefined(order.Side))
            errors["side"] = ["O lado deve ser Compra ou Venda."];

        if (order.Quantity <= 0 || order.Quantity >= MaxQuantityExclusive)
            errors["quantity"] = [$"A quantidade deve ser maior que zero e menor que {MaxQuantityExclusive}."];

        if (order.Price <= 0 || order.Price >= MaxPriceExclusive || order.Price % PriceTick != 0)
            errors["price"] = [$"O preço deve ser maior que zero, menor que {MaxPriceExclusive} e múltiplo de {PriceTick}."];

        return errors;
    }
}