namespace AnvilCore.SampleApi;

public sealed class AlwaysSucceedPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(decimal amount) =>
        Task.FromResult(new PaymentResult(true));
}
