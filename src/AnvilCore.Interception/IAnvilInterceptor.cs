namespace AnvilCore.Interception;

public delegate ValueTask<object?> AnvilNextDelegate(AnvilInvocationContext context);

public interface IAnvilInterceptor
{
    ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default);
}
