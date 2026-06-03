# AnvilCore.Testcontainers

Base package for Testcontainers support in AnvilCore. Provides the `IAnvilResource` abstraction and `AddResource()` extension, which handles container startup, configuration injection, and guaranteed cleanup tied to the test application lifetime.

## Installation

```
dotnet add package AnvilCore.Testcontainers
```

This package is typically not consumed directly. Use the resource-specific packages instead:

- `AnvilCore.Testcontainers.Postgres`
- `AnvilCore.Testcontainers.Redis`
- `AnvilCore.Testcontainers.RabbitMq`

## Custom resources

Implement `IAnvilResource` to integrate any Testcontainers-backed service:

```csharp
public sealed class MyCustomResource : IAnvilResource
{
    private readonly MyContainer _container = new MyContainerBuilder().Build();

    public string Name => "my-service";

    public ValueTask StartAsync(CancellationToken ct) => new(_container.StartAsync(ct));
    public ValueTask StopAsync(CancellationToken ct)  => new(_container.StopAsync(ct));

    public void ConfigureApplication(IConfigurationBuilder config) =>
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:MyService"] = _container.GetConnectionString()
        });

    public void ConfigureServices(IServiceCollection services) { }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
```

Then register it:

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .AddResource(new MyCustomResource())
    .BuildAsync();
```

## Lifecycle

1. `StartAsync` is called before the host builds — containers are running before services are resolved.
2. Connection strings from `ConfigureApplication` are injected into both the plain-DI `IConfiguration` and the ASP.NET Core `ConfigureAppConfiguration` pipeline.
3. `StopAsync` and `DisposeAsync` are called when the test application disposes, even if the host threw during teardown.

## Dependencies

- `AnvilCore`
- `Testcontainers`
- `Microsoft.Extensions.Configuration`
