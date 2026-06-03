# AnvilCore.NSubstitute

Wires NSubstitute into AnvilCore as the `IAnvilSubstituteProvider`. Lets tests register auto-generated NSubstitute substitutes for services that have no real implementation in the test host.

## Installation

```
dotnet add package AnvilCore.NSubstitute
```

## Usage

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseNSubstitute()
    .AddSubstitute<IMyService>()
    .BuildAsync();

var sub = app.Services.GetRequiredService<IMyService>();
sub.DoWork().Returns(42); // configure via NSubstitute API
```

`UseNSubstitute()` registers the provider. `AddSubstitute<T>()` registers a substitute for a specific interface as a singleton in the DI container.

### Substitute vs Intercept

| | `AddSubstitute<T>()` | `Intercept<T>()` |
|---|---|---|
| Replaces real impl | Yes — no real impl needed | No — wraps the existing registration |
| Configure via NSubstitute | Yes | No |
| Capture / Suppress / Return | No | Yes |

Use `AddSubstitute<T>()` when the service has no real implementation in the test environment. Use `Intercept<T>()` when you want to observe or modify calls to an existing registration.

## Dependencies

- `AnvilCore`
- `NSubstitute`
