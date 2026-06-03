# AnvilCore.AutoFixture

Wires AutoFixture into AnvilCore as the `IAnvilData` provider. Enables `app.Data.Create<T>()` and powers `ReturnFixture()` in interception pipelines.

## Installation

```
dotnet add package AnvilCore.AutoFixture
```

## Usage

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseWebApplicationFactory<Program>()
    .UseAutoFixture()
    .BuildAsync();

var request = app.Data.Create<CreateReservationRequest>();
var many    = app.Data.CreateMany<CreateReservationRequest>(5);
```

Call `UseAutoFixture()` before any interceptor that uses `ReturnFixture()`.

### Custom fixture

If you need a customised `IFixture`, register `AutoFixtureAnvilData` directly:

```csharp
var fixture = new Fixture().Customize(new MyCustomization());

builder.ConfigureServices(services =>
    services.AddSingleton<IAnvilData>(new AutoFixtureAnvilData(fixture)));
```

## Dependencies

- `AnvilCore`
- `AutoFixture`
