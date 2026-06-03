# AnvilCore.Testcontainers.Redis

Redis Testcontainers resource for AnvilCore. Starts a real Redis container and injects the connection string into the test application's configuration before the host builds.

## Installation

```
dotnet add package AnvilCore.Testcontainers.Redis
```

## Usage

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseWebApplicationFactory<Program>()
    .AddRedis("cache")
    .BuildAsync();
```

The connection string is available at `ConnectionStrings:cache` in `IConfiguration`.

### Custom configuration key

```csharp
.AddRedis("cache", configurationKey: "ConnectionStrings:Redis")
```

## Dependencies

- `AnvilCore.Testcontainers`
- `Testcontainers.Redis`
