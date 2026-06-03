using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore;

public interface IAnvilTestApplicationBuilder
{
    IServiceCollection Services { get; }
    IDictionary<string, object> Properties { get; }

    IAnvilTestApplicationBuilder AddModule(IAnvilModule module);
    IAnvilTestApplicationBuilder ConfigureServices(Action<IServiceCollection> configure);
    IAnvilTestApplicationBuilder ApplyScenario<TScenario>() where TScenario : IAnvilScenario, new();
    IAnvilTestApplicationBuilder ApplyScenario(IAnvilScenario scenario);
    ValueTask<IAnvilTestApplication> BuildAsync(CancellationToken cancellationToken = default);
}
