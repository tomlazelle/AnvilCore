namespace AnvilCore.Interception;

internal sealed class ReturnValueInterceptor(object? value) : IAnvilInterceptor
{
    public ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(value);
    }
}
