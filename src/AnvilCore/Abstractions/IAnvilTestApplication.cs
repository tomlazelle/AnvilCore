using System.Net.Http;

namespace AnvilCore;

public interface IAnvilTestApplication : IAsyncDisposable
{
    IServiceProvider Services { get; }
    IAnvilData Data { get; }
    IAnvilCaptureStore Captured { get; }

    HttpClient CreateClient() => throw new InvalidOperationException(
        "CreateClient() requires a web application. Use UseWebApplicationFactory<TEntryPoint>() when building.");
}
