using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AnvilCore.Testcontainers.Postgres;

public sealed class AnvilPostgresResource : IAnvilResource
{
    private readonly AnvilPostgresOptions _options;
    private readonly PostgreSqlContainer _container;

    public string Name { get; }

    public AnvilPostgresResource(string name, AnvilPostgresOptions options)
    {
        Name = name;
        _options = options;
        _container = new PostgreSqlBuilder()
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
            [_options.ConfigurationKey] = _container.GetConnectionString()
        });

    public void ConfigureServices(IServiceCollection services) { }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
