namespace AnvilCore.Interception;

public interface IAnvilProxyFactory
{
    object CreateProxy(Type serviceType, object inner, AnvilInterceptorPipeline pipeline, IServiceProvider services);
}
