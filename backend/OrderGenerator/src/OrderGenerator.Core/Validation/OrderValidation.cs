using OrderGenerator.Core.Orders;

namespace OrderGenerator.Core.Validation;

public static class OrderValidator
{
	public static readonly IReadOnlySet<string> AllowedSymbols = new HashSet<string> { "PETR4", "VALE3", "VIIA4" };
	public const int MaxQuantityExclusive = 100_000;
	public const decimal MaxPriceExclusive = 1_000m;
	public const decimal PriceTick = 0.01m;

	public static IReadOnlyDictionary<string, string[]> Validate(Order order) => throw new NotImplementedException();
}