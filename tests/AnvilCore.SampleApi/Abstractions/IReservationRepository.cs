namespace AnvilCore.SampleApi;

public interface IReservationRepository
{
    Task SaveAsync(Reservation reservation);
    IReadOnlyList<Reservation> GetAll();
}
