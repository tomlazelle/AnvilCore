# AnvilCore.AspNetCore

Adds ASP.NET Core support to AnvilCore via `WebApplicationFactory<T>`. Enables full in-process HTTP testing with all AnvilCore features (interception, capture, data generation) available inside the running host.

## Installation

```
dotnet add package AnvilCore.AspNetCore
```

## Usage

Call `UseWebApplicationFactory<TEntryPoint>()` on the builder, then call `app.CreateClient()` to get an `HttpClient` pointed at the in-process host:

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseWebApplicationFactory<Program>()
    .BuildAsync();

var client = app.CreateClient();
var response = await client.GetAsync("/health");
response.StatusCode.ShouldBe(HttpStatusCode.OK);
```

`TEntryPoint` is the entry-point class of your web application (typically `Program`).

## How it works

- The host runs in the `"Test"` environment.
- All services registered on the builder (via `ConfigureServices`, `UseAutoFixture`, `Intercept<T>`, etc.) are merged into the web host's DI container.
- Resource configuration (connection strings from Testcontainers, etc.) is injected via `ConfigureAppConfiguration` so it merges with `appsettings.json`.

## Dependencies

- `AnvilCore`
- `Microsoft.AspNetCore.Mvc.Testing`
