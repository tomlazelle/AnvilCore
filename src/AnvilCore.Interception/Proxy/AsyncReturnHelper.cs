using System.Reflection;

namespace AnvilCore.Interception;

internal static class AsyncReturnHelper
{
    internal static readonly MethodInfo CastTaskResultMethod =
        typeof(AsyncReturnHelper).GetMethod(nameof(CastTaskResult), BindingFlags.NonPublic | BindingFlags.Static)!;

    internal static readonly MethodInfo CastValueTaskResultMethod =
        typeof(AsyncReturnHelper).GetMethod(nameof(CastValueTaskResult), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static async Task<T> CastTaskResult<T>(Task<object?> source)
    {
        var result = await source;
        return result is null ? default! : (T)result;
    }

    private static async ValueTask<T> CastValueTaskResult<T>(Task<object?> source)
    {
        var result = await source;
        return result is null ? default! : (T)result;
    }
}
