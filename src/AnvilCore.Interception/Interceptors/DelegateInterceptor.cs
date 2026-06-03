namespace AnvilCore.Interception;

internal sealed class DelegateInterceptor(
    Func<AnvilInvocationContext, AnvilNextDelegate, CancellationToken, ValueTask<object?>> handler) : IAnvilInterceptor
{
    public ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default)
    {
        return handler(context, next, cancellationToken);
    }
}
