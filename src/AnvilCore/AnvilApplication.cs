using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore;

internal sealed class AnvilApplication : IAnvilTestApplication
{
    private readonly ServiceProvider _serviceProvider;

    internal AnvilApplication(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Services = serviceProvider;
        Data = serviceProvider.GetRequiredService<IAnvilData>();
        Captured = serviceProvider.GetRequiredService<IAnvilCaptureStore>();
    }

    public IServiceProvider Services { get; }
    public IAnvilData Data { get; }
    public IAnvilCaptureStore Captured { get; }

    public ValueTask DisposeAsync() => _serviceProvider.DisposeAsync();
}
