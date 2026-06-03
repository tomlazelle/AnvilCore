namespace AnvilCore;

public interface IAnvilCaptureStore
{
    void Add(AnvilCapturedInvocation invocation);
    IReadOnlyList<AnvilCapturedInvocation> Invocations { get; }
    IReadOnlyList<object?> Messages { get; }
    IReadOnlyList<T> ArgumentsOfType<T>();
    IReadOnlyList<T> ReturnsOfType<T>();
    IReadOnlyList<AnvilCapturedInvocation> InvocationsFor<TService>();
}
