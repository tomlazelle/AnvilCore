namespace AnvilCore.Interception;

public sealed class AnvilInterceptorPipeline
{
    private readonly IReadOnlyList<IAnvilInterceptor> _interceptors;

    internal AnvilInterceptorPipeline(IReadOnlyList<IAnvilInterceptor> interceptors)
    {
        _interceptors = interceptors;
    }

    public ValueTask<object?> ExecuteAsync(AnvilInvocationContext context, CancellationToken cancellationToken = default)
        => ExecuteAtIndex(0, context, cancellationToken);

    private ValueTask<object?> ExecuteAtIndex(int index, AnvilInvocationContext context, CancellationToken cancellationToken)
    {
        if (index >= _interceptors.Count)
            return ValueTask.FromResult<object?>(null);

        var interceptor = _interceptors[index];
        AnvilNextDelegate next = ctx => ExecuteAtIndex(index + 1, ctx, cancellationToken);
        return interceptor.InvokeAsync(context, next, cancellationToken);
    }
}
