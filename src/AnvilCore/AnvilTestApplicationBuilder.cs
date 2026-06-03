using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore;

public sealed class AnvilTestApplicationBuilder : IAnvilTestApplicationBuilder
{
    private const string PostConfigureActionsKey = AnvilBuilderProperties.PostConfigureActions;
    private const string PreBuildAsyncActionsKey = AnvilBuilderProperties.PreBuildAsyncActions;
    private const string AppDisposeCallbacksKey  = AnvilBuilderProperties.AppDisposeCallbacks;
    private const string ResourceConfigKey       = AnvilBuilderProperties.ResourceConfig;

    private IAnvilHostStrategy? _hostStrategy;

    public IServiceCollection Services { get; } = new ServiceCollection();
    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

    public IAnvilTestApplicationBuilder AddModule(IAnvilModule module)
    {
        module.Configure(this);
        return this;
    }

    public IAnvilTestApplicationBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        configure(Services);
        return this;
    }

    public IAnvilTestApplicationBuilder ApplyScenario<TScenario>() where TScenario : IAnvilScenario, new()
        => ApplyScenario(new TScenario());

    public IAnvilTestApplicationBuilder ApplyScenario(IAnvilScenario scenario)
    {
        scenario.Configure(this);
        return this;
    }

    public AnvilTestApplicationBuilder UseHostStrategy(IAnvilHostStrategy strategy)
    {
        _hostStrategy = strategy;
        return this;
    }

    public async ValueTask<IAnvilTestApplication> BuildAsync(CancellationToken cancellationToken = default)
    {
        if (_hostStrategy is not null)
        {
            // Pre-build async actions (resource startup) run here so containers are up
            // before the host strategy creates the web application factory.
            await RunPreBuildAsyncActionsAsync(cancellationToken);
            EnsureInternalServices();
            var webApp = await _hostStrategy.BuildAsync(this, cancellationToken);
            return WrapWithDisposeCallbacks(webApp);
        }

        await RunPreBuildAsyncActionsAsync(cancellationToken);
        RunPostConfigureActions();
        EnsureInternalServices();

        var serviceProvider = Services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });

        return WrapWithDisposeCallbacks(new AnvilApplication(serviceProvider));
    }

    private async Task RunPreBuildAsyncActionsAsync(CancellationToken cancellationToken)
    {
        if (Properties.TryGetValue(PreBuildAsyncActionsKey, out var obj) &&
            obj is List<Func<CancellationToken, ValueTask>> actions)
        {
            foreach (var action in actions)
                await action(cancellationToken);
        }
    }

    private void RunPostConfigureActions()
    {
        if (Properties.TryGetValue(PostConfigureActionsKey, out var obj) &&
            obj is List<Action<IServiceCollection>> actions)
        {
            foreach (var action in actions)
                action(Services);
        }
    }

    private void EnsureInternalServices()
    {
        if (!Services.Any(s => s.ServiceType == typeof(IAnvilCaptureStore)))
            Services.AddSingleton<IAnvilCaptureStore, AnvilCaptureStore>();

        if (!Services.Any(s => s.ServiceType == typeof(IAnvilData)))
            Services.AddSingleton<IAnvilData, NullAnvilData>();
    }

    private IAnvilTestApplication WrapWithDisposeCallbacks(IAnvilTestApplication app)
    {
        if (!Properties.TryGetValue(AppDisposeCallbacksKey, out var obj) ||
            obj is not List<Func<ValueTask>> callbacks || callbacks.Count == 0)
            return app;

        return new ResourceAwareAnvilApplication(app, callbacks);
    }
}
