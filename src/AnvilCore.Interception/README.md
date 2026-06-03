# AnvilCore.Interception

Adds DI service interception to AnvilCore. Wraps any registered interface service in a `DispatchProxy` pipeline so tests can capture, suppress, replace, or delay calls without touching production code.

## Installation

```
dotnet add package AnvilCore.Interception
```

## Usage

Call `Intercept<TService>()` on the builder and compose the pipeline with the available interceptors:

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseWebApplicationFactory<Program>()
    .Intercept<IPaymentGateway>(x => x.Return(new PaymentResult(false, "Declined")))
    .BuildAsync();
```

```csharp
// Capture invocations then suppress the real call
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseWebApplicationFactory<Program>()
    .Intercept<IMessageBus>(x => { x.Capture(); x.Suppress(); })
    .BuildAsync();

// After the test:
app.Captured.ArgumentsOfType<ReservationCreated>().Count.ShouldBe(1);
```

## Interceptors

| Method | Behaviour |
|--------|-----------|
| `Capture()` | Records the invocation (arguments, return value, timing) to `app.Captured` |
| `Suppress()` | Stops the pipeline and returns `null` / `default` |
| `Continue()` | Calls through to the real implementation |
| `Return(value)` | Returns a fixed value |
| `ReturnFixture()` | Generates a return value via `IAnvilData` (requires `UseAutoFixture()`) |
| `Throw(exception)` | Throws the supplied exception |
| `Delay(duration)` | Waits before continuing the pipeline |
| `Use(handler)` | Custom delegate interceptor |

Interceptors compose left-to-right. `Capture()` must come before `Suppress()` or `Continue()` to record the outcome.

## Constraints

- Only interface services can be intercepted.
- The service must already be registered before `Intercept<T>()` is called.
