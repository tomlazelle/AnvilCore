namespace AnvilCore.Interception;

internal sealed class ThrowInterceptor(Exception exception) : IAnvilInterceptor
{
    public ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default)
    {
        throw exception;
    }
}
