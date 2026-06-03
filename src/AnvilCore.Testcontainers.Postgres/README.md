# AnvilCore.Testcontainers.Postgres

PostgreSQL Testcontainers resource for AnvilCore. Starts a real PostgreSQL container and injects the connection string into the test application's configuration before the host builds.

## Installation

```
dotnet add package AnvilCore.Testcontainers.Postgres
```

## Usage

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseWebApplicationFactory<Program>()
    .AddPostgres("db")
    .BuildAsync();
```

The connection string is available at `ConnectionStrings:db` in `IConfiguration`.

### Custom configuration key

```csharp
.AddPostgres("db", options =>
{
    options.ConfigurationKey = "ConnectionStrings:MyDatabase";
})
```

### Multiple databases

Each resource must have a unique name:

```csharp
.AddPostgres("primary")
.AddPostgres("readonly", o => o.ConfigurationKey = "ConnectionStrings:ReadOnly")
```

## Dependencies

- `AnvilCore.Testcontainers`
- `Testcontainers.PostgreSql`
