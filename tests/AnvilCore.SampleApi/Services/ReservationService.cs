namespace AnvilCore.SampleApi;

public sealed class ReservationService(
    IPaymentGateway paymentGateway,
    IReservationRepository repository,
    IMessageBus bus) : IReservationService
{
    public async Task<Reservation?> CreateAsync(CreateReservationRequest request)
    {
        var payment = await paymentGateway.ChargeAsync(request.Amount);
        if (!payment.Success)
            return null;

        var reservation = new Reservation(Guid.NewGuid(), request.GuestName, request.Amount);
        await repository.SaveAsync(reservation);
        bus.Publish(new ReservationCreated(reservation.Id, reservation.GuestName));
        return reservation;
    }
}
