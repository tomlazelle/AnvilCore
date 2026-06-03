namespace AnvilCore.SampleApi;

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(decimal amount);
}
