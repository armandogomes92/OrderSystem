namespace OrderAccumulator.Core.Exposure;

public sealed record ExposureDecision(bool Accepted, decimal Exposure, string? RejectReason = null);