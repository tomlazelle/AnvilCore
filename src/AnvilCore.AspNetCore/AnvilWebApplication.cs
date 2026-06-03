using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.AspNetCore;

public sealed class AnvilWebApplication<TEntryPoint> : IAnvilTestApplication
    where TEntryPoint : class
{
    private readonly AnvilWebApplicationFactory<TEntryPoint> _factory;

    internal AnvilWebApplication(AnvilWebApplicationFactory<TEntryPoint> factory)
    {
        _factory = factory;
        Services = factory.Services;
        Data = factory.Services.GetRequiredService<IAnvilData>();
        Captured = factory.Services.GetRequiredService<IAnvilCaptureStore>();
    }

    public IServiceProvider Services { get; }
    public IAnvilData Data { get; }
    public IAnvilCaptureStore Captured { get; }

    public HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = true
    });

    public ValueTask DisposeAsync()
    {
        _factory.Dispose();
        return ValueTask.CompletedTask;
    }
}
