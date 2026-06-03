using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.Testcontainers;

public interface IAnvilResource : IAsyncDisposable
{
    string Name { get; }
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
    void ConfigureApplication(IConfigurationBuilder configuration);
    void ConfigureServices(IServiceCollection services);
}
