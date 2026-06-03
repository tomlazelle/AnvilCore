namespace AnvilCore.Interception;

internal sealed class DelayInterceptor(TimeSpan duration) : IAnvilInterceptor
{
    public async ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(duration, cancellationToken);
        return await next(context);
    }
}
