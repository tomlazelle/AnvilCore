# AnvilCore.Xunit

xUnit integration for AnvilCore. Provides a base fixture class for shared application lifetime across a test class collection, and a logging extension that routes AnvilCore diagnostics to xUnit's `ITestOutputHelper`.

## Installation

```
dotnet add package AnvilCore.Xunit
```

## Usage

### Shared fixture

Derive from `AnvilXunitFixture<TEntryPoint>` and override `Configure` to set up the builder. The fixture starts the application once per collection and disposes it after all tests in the collection finish.

```csharp
public sealed class ApiFixture : AnvilXunitFixture<Program>
{
    protected override void Configure(IAnvilTestApplicationBuilder builder)
    {
        builder
            .UseAutoFixture()
            .Intercept<IMessageBus>(x => x.Suppress());
    }
}

[Collection(nameof(ApiFixture))]
public sealed class ReservationTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task Creates_reservation()
    {
        var client = fixture.Application.CreateClient();
        // ...
    }
}
```

### Per-test logging

Route AnvilCore log messages to the xUnit output window:

```csharp
await using var app = await AnvilTestApplication.CreateBuilder()
    .UseWebApplicationFactory<Program>()
    .UseXUnit(output)
    .BuildAsync();
```

## Dependencies

- `AnvilCore`
- `AnvilCore.AspNetCore`
- `xunit`
