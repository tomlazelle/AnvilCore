# AnvilCore.Testcontainers.RabbitMq

RabbitMQ Testcontainers resource for AnvilCore. Starts a real RabbitMQ container and injects the connection string into the test application's configuration before the host builds.

## Installation

```
dotnet add package AnvilCore.Testcontainers.RabbitMq
```

## Usage

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseWebApplicationFactory<Program>()
    .AddRabbitMq("messaging")
    .BuildAsync();
```

The connection string is available at `ConnectionStrings:messaging` in `IConfiguration`.

### Custom configuration key

```csharp
.AddRabbitMq("messaging", configurationKey: "ConnectionStrings:RabbitMq")
```

## Dependencies

- `AnvilCore.Testcontainers`
- `Testcontainers.RabbitMq`
