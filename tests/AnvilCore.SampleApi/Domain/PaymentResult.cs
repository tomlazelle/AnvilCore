namespace AnvilCore.SampleApi;

public sealed record PaymentResult(bool Success, string? Reason = null);
