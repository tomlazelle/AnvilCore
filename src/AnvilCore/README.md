# AnvilCore

The core package for AnvilCore. Provides the builder, application lifecycle, capture store, and the extension points that all other AnvilCore packages build on.

## Installation

```
dotnet add package AnvilCore
```

## Usage

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IMyService, MyService>();
    })
    .BuildAsync();

var svc = app.Services.GetRequiredService<IMyService>();
```

### Scenarios

Scenarios are reusable configuration recipes applied to the builder before it builds:

```csharp
public sealed class PaymentDeclinedScenario : IAnvilScenario
{
    public void Configure(IAnvilTestApplicationBuilder builder)
    {
        // configure services for this scenario
    }
}

await using var app = await AnvilTestApplication.CreateBuilder()
    .ApplyScenario<PaymentDeclinedScenario>()
    .BuildAsync();
```

### Modules

Modules are reusable bundles of builder configuration that can be shared across test projects:

```csharp
public sealed class MyModule : IAnvilModule
{
    public void Configure(IAnvilTestApplicationBuilder builder)
    {
        builder.ConfigureServices(services => { /* ... */ });
    }
}

await using var app = await AnvilTestApplication.CreateBuilder()
    .AddModule(new MyModule())
    .BuildAsync();
```

### Capture store

`app.Captured` records invocations when an interceptor pipeline includes `Capture()`. See `AnvilCore.Interception` for details.

## Key types

| Type | Purpose |
|------|---------|
| `AnvilTestApplication` | Entry point — call `CreateBuilder()` |
| `AnvilTestApplicationBuilder` | Fluent builder |
| `IAnvilTestApplication` | Built application; exposes `Services`, `Data`, `Captured` |
| `IAnvilModule` | Reusable builder configuration bundle |
| `IAnvilScenario` | Reusable environment/configuration recipe |
| `IAnvilData` | Test data factory (implemented by `AnvilCore.AutoFixture`) |
| `IAnvilCaptureStore` | Invocation capture store |
