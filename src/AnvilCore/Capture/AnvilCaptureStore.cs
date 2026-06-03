using System.Collections.Concurrent;

namespace AnvilCore;

internal sealed class AnvilCaptureStore : IAnvilCaptureStore
{
    private readonly ConcurrentQueue<AnvilCapturedInvocation> _invocations = new();

    public void Add(AnvilCapturedInvocation invocation) => _invocations.Enqueue(invocation);

    public IReadOnlyList<AnvilCapturedInvocation> Invocations => _invocations.ToList();

    public IReadOnlyList<object?> Messages =>
        _invocations.SelectMany(i => i.Arguments).ToList();

    public IReadOnlyList<T> ArgumentsOfType<T>() =>
        _invocations.SelectMany(i => i.Arguments).OfType<T>().ToList();

    public IReadOnlyList<T> ReturnsOfType<T>() =>
        _invocations.Select(i => i.ReturnValue).OfType<T>().ToList();

    public IReadOnlyList<AnvilCapturedInvocation> InvocationsFor<TService>() =>
        _invocations.Where(i => i.ServiceType == typeof(TService)).ToList();
}
