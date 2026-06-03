namespace AnvilCore.Interception;

internal sealed class ContinueInterceptor : IAnvilInterceptor
{
    public ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default)
    {
        return context.InvokeInner(context.Arguments);
    }
}
