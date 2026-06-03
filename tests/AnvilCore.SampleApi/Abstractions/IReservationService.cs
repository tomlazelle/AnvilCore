namespace AnvilCore.SampleApi;

public interface IReservationService
{
    Task<Reservation?> CreateAsync(CreateReservationRequest request);
}
