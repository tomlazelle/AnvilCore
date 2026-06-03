namespace AnvilCore.Interception;

internal sealed class SuppressInterceptor : IAnvilInterceptor
{
    public ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<object?>(null);
    }
}
