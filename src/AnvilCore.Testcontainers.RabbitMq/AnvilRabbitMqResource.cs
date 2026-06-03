using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.RabbitMq;

namespace AnvilCore.Testcontainers.RabbitMq;

public sealed class AnvilRabbitMqResource : IAnvilResource
{
    private readonly string _configurationKey;
    private readonly RabbitMqContainer _container;

    public string Name { get; }

    public AnvilRabbitMqResource(string name, string configurationKey)
    {
        Name = name;
        _configurationKey = configurationKey;
        _container = new RabbitMqBuilder()
            .WithAutoRemove(true)
            .Build();
    }

    public ValueTask StartAsync(CancellationToken cancellationToken = default) =>
        new(_container.StartAsync(cancellationToken));

    public ValueTask StopAsync(CancellationToken cancellationToken = default) =>
        new(_container.StopAsync(cancellationToken));

    public void ConfigureApplication(IConfigurationBuilder configuration) =>
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [_configurationKey] = _container.GetConnectionString()
        });

    public void ConfigureServices(IServiceCollection services) { }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
