namespace OrderGenerator.Api.Fix;

public sealed class FixSessionUnavailableException()
    : Exception("A sessão FIX com o OrderAccumulator não está conectada.");