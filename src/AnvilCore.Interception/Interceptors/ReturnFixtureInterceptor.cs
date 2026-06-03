namespace AnvilCore.Interception;

internal sealed class ReturnFixtureInterceptor : IAnvilInterceptor
{
    public ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default)
    {
        var resultType = GetResultType(context.ReturnType);

        if (resultType is null)
            return ValueTask.FromResult<object?>(null);

        return ValueTask.FromResult<object?>(context.Data.Create(resultType));
    }

    private static Type? GetResultType(Type returnType)
    {
        if (returnType == typeof(void) || returnType == typeof(Task) || returnType == typeof(ValueTask))
            return null;

        if (returnType.IsGenericType)
        {
            var def = returnType.GetGenericTypeDefinition();
            if (def == typeof(Task<>) || def == typeof(ValueTask<>))
                return returnType.GetGenericArguments()[0];
        }

        return returnType;
    }
}
