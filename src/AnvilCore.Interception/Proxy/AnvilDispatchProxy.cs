using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.Interception;

internal class AnvilDispatchProxy<TService> : DispatchProxy
    where TService : class
{
    private TService _inner = default!;
    private AnvilInterceptorPipeline _pipeline = default!;
    private IServiceProvider _services = default!;

    internal static TService Create(TService inner, AnvilInterceptorPipeline pipeline, IServiceProvider services)
    {
        var proxy = Create<TService, AnvilDispatchProxy<TService>>();
        var anvilProxy = (AnvilDispatchProxy<TService>)(object)proxy;
        anvilProxy._inner = inner;
        anvilProxy._pipeline = pipeline;
        anvilProxy._services = services;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var method = targetMethod!;
        var arguments = args ?? [];
        var returnType = method.ReturnType;

        var context = new AnvilInvocationContext
        {
            ServiceType = typeof(TService),
            Method = method,
            Arguments = arguments,
            Services = _services,
            Data = _services.GetRequiredService<IAnvilData>(),
            Captured = _services.GetRequiredService<IAnvilCaptureStore>(),
            InvokeInner = invokeArgs => InvokeInnerAsync(_inner, method, invokeArgs, returnType)
        };

        return UnwrapResult(_pipeline.ExecuteAsync(context), returnType);
    }

    private static async ValueTask<object?> InvokeInnerAsync(TService inner, MethodInfo method, object?[] args, Type returnType)
    {
        var raw = method.Invoke(inner, args);

        if (returnType == typeof(void)) return null;
        if (returnType == typeof(Task)) { await (Task)raw!; return null; }
        if (returnType == typeof(ValueTask)) { await (ValueTask)raw!; return null; }

        if (returnType.IsGenericType)
        {
            var def = returnType.GetGenericTypeDefinition();

            if (def == typeof(Task<>))
            {
                var task = (Task)raw!;
                await task;
                return returnType.GetProperty("Result")!.GetValue(task);
            }

            if (def == typeof(ValueTask<>))
            {
                var asTask = (Task)returnType.GetMethod("AsTask")!.Invoke(raw, null)!;
                await asTask;
                return asTask.GetType().GetProperty("Result")!.GetValue(asTask);
            }
        }

        return raw;
    }

    private static object? UnwrapResult(ValueTask<object?> pipelineTask, Type returnType)
    {
        if (returnType == typeof(void))
        {
            pipelineTask.AsTask().GetAwaiter().GetResult();
            return null;
        }

        if (returnType == typeof(Task))
            return pipelineTask.AsTask();

        if (returnType == typeof(ValueTask))
            return new ValueTask(pipelineTask.AsTask());

        if (returnType.IsGenericType)
        {
            var def = returnType.GetGenericTypeDefinition();
            var typeArg = returnType.GetGenericArguments()[0];

            if (def == typeof(Task<>))
                return AsyncReturnHelper.CastTaskResultMethod
                    .MakeGenericMethod(typeArg)
                    .Invoke(null, [pipelineTask.AsTask()]);

            if (def == typeof(ValueTask<>))
                return AsyncReturnHelper.CastValueTaskResultMethod
                    .MakeGenericMethod(typeArg)
                    .Invoke(null, [pipelineTask.AsTask()]);
        }

        return pipelineTask.AsTask().GetAwaiter().GetResult();
    }
}
