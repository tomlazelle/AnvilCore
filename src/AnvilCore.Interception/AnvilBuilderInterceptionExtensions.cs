using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.Interception;

public static class AnvilBuilderInterceptionExtensions
{
    private const string PostConfigureActionsKey = "anvil:post-configure-actions";

    public static IAnvilTestApplicationBuilder Intercept<TService>(
        this IAnvilTestApplicationBuilder builder,
        Action<InterceptionPipelineBuilder> configure)
        where TService : class
    {
        if (!typeof(TService).IsInterface)
            throw new AnvilInterceptionException(
                $"Cannot intercept {typeof(TService).Name}: only interface services are supported in v1.");

        var pipelineBuilder = new InterceptionPipelineBuilder();
        configure(pipelineBuilder);

        AddPostConfigureAction(builder, services =>
        {
            AnvilBuilderProperties.Log(builder.Properties,
                $"[AnvilCore] Applying interceptor for {typeof(TService).Name}");
            DecorateService<TService>(services, pipelineBuilder);
        });
        return builder;
    }

    private static void AddPostConfigureAction(IAnvilTestApplicationBuilder builder, Action<IServiceCollection> action)
    {
        if (!builder.Properties.TryGetValue(PostConfigureActionsKey, out var obj) ||
            obj is not List<Action<IServiceCollection>>)
        {
            obj = new List<Action<IServiceCollection>>();
            builder.Properties[PostConfigureActionsKey] = obj;
        }

        ((List<Action<IServiceCollection>>)obj).Add(action);
    }

    private static void DecorateService<TService>(IServiceCollection services, InterceptionPipelineBuilder pipelineBuilder)
        where TService : class
    {
        var descriptor = services.LastOrDefault(s => s.ServiceType == typeof(TService))
            ?? throw new AnvilInterceptionException(
                $"Service {typeof(TService).Name} could not be intercepted because no registration was found. " +
                $"Register the service first or call AddSubstitute<{typeof(TService).Name}>().");

        services.Remove(descriptor);

        var pipeline = pipelineBuilder.Build();
        var proxyFactory = new DispatchProxyFactory();

        services.Add(new ServiceDescriptor(
            typeof(TService),
            sp =>
            {
                var inner = BuildInner<TService>(sp, descriptor);
                return proxyFactory.CreateProxy(typeof(TService), inner, pipeline, sp);
            },
            descriptor.Lifetime));
    }

    private static TService BuildInner<TService>(IServiceProvider sp, ServiceDescriptor descriptor)
        where TService : class
    {
        if (descriptor.ImplementationInstance is TService instance)
            return instance;

        if (descriptor.ImplementationFactory is not null)
            return (TService)descriptor.ImplementationFactory(sp);

        if (descriptor.ImplementationType is not null)
            return (TService)ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType);

        throw new AnvilInterceptionException(
            $"Cannot create inner instance for {typeof(TService).Name}: unsupported service descriptor.");
    }
}
