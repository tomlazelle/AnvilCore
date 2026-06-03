namespace AnvilCore.Interception;

internal sealed class CaptureInterceptor : IAnvilInterceptor
{
    public async ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        Exception? exception = null;
        object? result = null;

        try
        {
            result = await next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            context.Captured.Add(new AnvilCapturedInvocation(
                context.ServiceType,
                context.Method,
                context.Arguments,
                result,
                exception,
                startedAt,
                DateTimeOffset.UtcNow));
        }

        return result;
    }
}
