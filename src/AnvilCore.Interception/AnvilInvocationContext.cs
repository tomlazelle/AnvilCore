using System.Reflection;

namespace AnvilCore.Interception;

public sealed class AnvilInvocationContext
{
    public required Type ServiceType { get; init; }
    public required MethodInfo Method { get; init; }
    public required object?[] Arguments { get; init; }
    public required IServiceProvider Services { get; init; }
    public required IAnvilData Data { get; init; }
    public required IAnvilCaptureStore Captured { get; init; }
    internal Func<object?[], ValueTask<object?>> InvokeInner { get; init; } =
        _ => throw new InvalidOperationException("InvokeInner was not configured.");

    public Type ReturnType => Method.ReturnType;

    public bool IsAsync =>
        ReturnType == typeof(Task) || ReturnType == typeof(ValueTask) ||
        (ReturnType.IsGenericType &&
            (ReturnType.GetGenericTypeDefinition() == typeof(Task<>) ||
             ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)));

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();
}
