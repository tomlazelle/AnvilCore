using System.Reflection;

namespace AnvilCore.Interception;

public sealed class DispatchProxyFactory : IAnvilProxyFactory
{
    private static readonly MethodInfo CreateHelperMethod =
        typeof(DispatchProxyFactory).GetMethod(nameof(CreateHelper), BindingFlags.NonPublic | BindingFlags.Static)!;

    public object CreateProxy(Type serviceType, object inner, AnvilInterceptorPipeline pipeline, IServiceProvider services)
    {
        return CreateHelperMethod.MakeGenericMethod(serviceType).Invoke(null, [inner, pipeline, services])!;
    }

    private static TService CreateHelper<TService>(TService inner, AnvilInterceptorPipeline pipeline, IServiceProvider services)
        where TService : class
    {
        return AnvilDispatchProxy<TService>.Create(inner, pipeline, services);
    }
}
