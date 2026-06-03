using System.Reflection;

namespace AnvilCore;

public sealed record AnvilCapturedInvocation(
    Type ServiceType,
    MethodInfo Method,
    object?[] Arguments,
    object? ReturnValue,
    Exception? Exception,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt);
