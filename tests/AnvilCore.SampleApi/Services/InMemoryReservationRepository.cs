using System.Collections.Concurrent;

namespace AnvilCore.SampleApi;

public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly ConcurrentBag<Reservation> _store = new();

    public Task SaveAsync(Reservation reservation)
    {
        _store.Add(reservation);
        return Task.CompletedTask;
    }

    public IReadOnlyList<Reservation> GetAll() => _store.ToList();
}
