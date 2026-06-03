namespace AnvilCore.SampleApi;

public sealed record Reservation(Guid Id, string GuestName, decimal Amount);
