# AnvilCore Design Specification

## 1. Executive Summary

AnvilCore is an Aspire-inspired .NET testing platform for application developers. Its goal is to let developers define a test application topology, explicitly choose infrastructure dependencies, and intercept dependency-injection-resolved services through a unified pipeline model.

AnvilCore is not a replacement for xUnit, Shouldly, NSubstitute, AutoFixture, Testcontainers, or ASP.NET Core testing. It is an orchestration and interception layer that coordinates those tools behind stable provider abstractions.

The first version targets .NET 10 LTS and xUnit only. It uses runtime behavior first. Source generators are intentionally deferred until the runtime model is proven.

## 2. Design Goals

### Primary Goals

1. Provide Aspire-like test composition for .NET applications.
2. Allow developers to explicitly add infrastructure such as Postgres, Redis, RabbitMQ, or other Testcontainers-backed services.
3. Allow dependency-injection-only interception of services registered in the application container.
4. Preserve Arrange, Act, Assert as the primary test style.
5. Keep xUnit, Shouldly, NSubstitute, AutoFixture, and Testcontainers replaceable through provider abstractions.
6. Keep application code unaware that it is under test.
7. Support both integration-style API tests and component/service-level tests from a single test project.

### Non-Goals for v1

1. No source generators.
2. No custom test runner.
3. No replacement assertion framework.
4. No replacement mocking framework.
5. No bytecode weaving or method interception outside DI.
6. No database query interception.
7. No HTTP client interception outside explicitly registered DI services.
8. No NUnit or MSTest support in v1.
9. No automatic infrastructure discovery in v1.

## 3. Target Platform

- Target framework: `net10.0`
- Test framework: xUnit
- Primary application host: ASP.NET Core `WebApplicationFactory<TEntryPoint>`
- Infrastructure runtime: Testcontainers for .NET
- Initial fixture provider: AutoFixture
- Initial substitute provider: NSubstitute
- Initial assertion package: Shouldly, but kept outside AnvilCore core

## 4. Conceptual Model

AnvilCore is built around four concepts:

```text
Test Application
    ├── Infrastructure Resources
    ├── Application Host
    ├── Service Interceptors
    └── Scenario Modules
```

The developer explicitly composes the test environment:

```csharp
await using var app = await AnvilTestApplication
    .CreateBuilder<Program>()
    .UseXUnit(output)
    .AddPostgres("db")
    .AddRedis("cache")
    .AddRabbitMq("broker")
    .Intercept<IMessageBus>(x => x.Capture().Continue())
    .Intercept<IPaymentGateway>(x => x.ReturnFixture())
    .BuildAsync();
```

Then the test remains normal Arrange, Act, Assert:

```csharp
[Fact]
public async Task CreateReservation_returns_created()
{
    // Arrange
    await using var app = await AnvilTestApplication
        .CreateBuilder<Program>()
        .AddPostgres("db")
        .Intercept<IMessageBus>(x => x.Capture().Suppress())
        .BuildAsync();

    var client = app.CreateClient();
    var request = app.Data.Create<CreateReservationRequest>();

    // Act
    var response = await client.PostAsJsonAsync("/reservations", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    app.Captured.Messages.OfType<ReservationCreated>().Count().ShouldBe(1);
}
```

## 5. Solution Structure

Use an Aspire-style package ecosystem.

```text
AnvilCore.sln

src/
  AnvilCore/
    Core abstractions and runtime model

  AnvilCore.AspNetCore/
    WebApplicationFactory integration
    Test server/client helpers

  AnvilCore.Xunit/
    xUnit lifecycle helpers
    IAsyncLifetime integration
    output logging integration

  AnvilCore.Interception/
    DI interception engine
    proxy pipeline
    invocation context

  AnvilCore.NSubstitute/
    NSubstitute provider implementation

  AnvilCore.AutoFixture/
    AutoFixture data provider implementation

  AnvilCore.Testcontainers/
    Testcontainers resource abstractions
    resource lifecycle orchestration

  AnvilCore.Testcontainers.Postgres/
    Postgres container support

  AnvilCore.Testcontainers.Redis/
    Redis container support

  AnvilCore.Testcontainers.RabbitMq/
    RabbitMQ container support

  AnvilCore.Diagnostics/
    logging, capture store, events, tracing hooks

tests/
  AnvilCore.Tests/
    core unit tests

  AnvilCore.AspNetCore.Tests/
    WebApplicationFactory tests

  AnvilCore.Interception.Tests/
    pipeline and proxy behavior tests

  AnvilCore.SampleApi/
    sample ASP.NET Core app used by tests

  AnvilCore.SampleApi.Tests/
    dogfood test project using AnvilCore

samples/
  BasicApiTesting/
  InterceptionOnly/
  TestcontainersPostgres/
  MessagingCapture/
```

## 6. Package Responsibilities

### AnvilCore

Contains the public core API and abstractions.

Key types:

```csharp
public interface IAnvilTestApplication : IAsyncDisposable
{
    IServiceProvider Services { get; }
    IAnvilData Data { get; }
    IAnvilCaptureStore Captured { get; }
}
```

```csharp
public interface IAnvilTestApplicationBuilder
{
    IServiceCollection Services { get; }
    IDictionary<string, object> Properties { get; }

    IAnvilTestApplicationBuilder AddModule(IAnvilModule module);
    IAnvilTestApplicationBuilder ConfigureServices(Action<IServiceCollection> configure);
    ValueTask<IAnvilTestApplication> BuildAsync(CancellationToken cancellationToken = default);
}
```

```csharp
public interface IAnvilModule
{
    void Configure(IAnvilTestApplicationBuilder builder);
}
```

### AnvilCore.AspNetCore

Provides ASP.NET Core host integration.

Key types:

```csharp
public sealed class AnvilWebApplication<TEntryPoint> : IAnvilTestApplication
    where TEntryPoint : class
{
    public HttpClient CreateClient();
    public IServiceProvider Services { get; }
    public IAnvilData Data { get; }
    public IAnvilCaptureStore Captured { get; }
}
```

Builder entry point:

```csharp
public static class AnvilTestApplication
{
    public static AnvilWebApplicationBuilder<TEntryPoint> CreateBuilder<TEntryPoint>()
        where TEntryPoint : class;
}
```

### AnvilCore.Interception

Provides DI-only interception.

Core requirement: any service resolved through DI may be wrapped with an interception proxy if the service is registered for interception.

Initial supported service shapes:

1. Interface services.
2. Public virtual class methods are optional and not required for v1.
3. Concrete class interception is deferred unless the selected proxy provider supports it cleanly.

Recommended v1 boundary: interface-only interception.

Key types:

```csharp
public delegate ValueTask<object?> AnvilNextDelegate(AnvilInvocationContext context);
```

```csharp
public interface IAnvilInterceptor
{
    ValueTask<object?> InvokeAsync(
        AnvilInvocationContext context,
        AnvilNextDelegate next,
        CancellationToken cancellationToken = default);
}
```

```csharp
public sealed class AnvilInvocationContext
{
    public required Type ServiceType { get; init; }
    public required MethodInfo Method { get; init; }
    public required object?[] Arguments { get; init; }
    public required IServiceProvider Services { get; init; }
    public required IAnvilData Data { get; init; }
    public required IAnvilCaptureStore Captured { get; init; }

    public Type ReturnType { get; }
    public bool IsAsync { get; }
    public IDictionary<string, object?> Items { get; }
}
```

Interception behavior examples:

```csharp
builder.Intercept<IMessageBus>(x => x
    .Capture()
    .Continue());
```

```csharp
builder.Intercept<IPaymentGateway>(x => x
    .ReturnFixture());
```

```csharp
builder.Intercept<IEmailSender>(x => x
    .Suppress());
```

```csharp
builder.Intercept<ICustomerRepository>(x => x
    .Use(async (ctx, next) =>
    {
        var result = await next(ctx);
        ctx.Captured.Add("repository.result", result);
        return result;
    }));
```

### AnvilCore.NSubstitute

Provides substitute creation behind an abstraction.

```csharp
public interface IAnvilSubstituteProvider
{
    object Create(Type serviceType);
    T Create<T>() where T : class;
}
```

NSubstitute implementation:

```csharp
public sealed class NSubstituteProvider : IAnvilSubstituteProvider
{
    public object Create(Type serviceType);
    public T Create<T>() where T : class;
}
```

### AnvilCore.AutoFixture

Provides data generation behind an abstraction.

```csharp
public interface IAnvilData
{
    object Create(Type type);
    T Create<T>();
    IEnumerable<T> CreateMany<T>(int count = 3);
}
```

AutoFixture implementation:

```csharp
public sealed class AutoFixtureAnvilData : IAnvilData
{
    public object Create(Type type);
    public T Create<T>();
    public IEnumerable<T> CreateMany<T>(int count = 3);
}
```

### AnvilCore.Testcontainers

Provides infrastructure orchestration.

```csharp
public interface IAnvilResource : IAsyncDisposable
{
    string Name { get; }
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
    void ConfigureApplication(IConfigurationBuilder configuration);
    void ConfigureServices(IServiceCollection services);
}
```

Example extension:

```csharp
public static IAnvilTestApplicationBuilder AddPostgres(
    this IAnvilTestApplicationBuilder builder,
    string name,
    Action<AnvilPostgresOptions>? configure = null);
```

## 7. DI Interception Strategy

### v1 Strategy

AnvilCore should intercept services at registration time by decorating matching DI service descriptors.

For example, production registration:

```csharp
services.AddScoped<IMessageBus, RabbitMqMessageBus>();
```

AnvilCore wraps it during test host configuration:

```text
IMessageBus
    -> AnvilProxy<IMessageBus>
        -> RabbitMqMessageBus
```

The test sees `IMessageBus` as normal. The application sees `IMessageBus` as normal. The interceptor pipeline receives method invocations before deciding whether to continue, return generated data, suppress, throw, delay, or capture.

### Decoration Rules

1. Preserve original service lifetime.
2. Preserve original implementation construction through DI.
3. Apply interception only when explicitly configured.
4. Support multiple interceptors per service.
5. Preserve service registration order where possible.
6. Fail fast when the service cannot be intercepted.

### Service Lifetime Behavior

| Original Lifetime | Proxy Lifetime | Inner Service Lifetime |
|---|---|---|
| Singleton | Singleton | Singleton |
| Scoped | Scoped | Scoped |
| Transient | Transient | Transient |

## 8. Interceptor Pipeline Semantics

Every intercepted invocation flows through a middleware-style pipeline.

```text
Application method call
    -> CaptureInterceptor
    -> DelayInterceptor
    -> FixtureReturnInterceptor
    -> ContinueInterceptor
    -> Real implementation
```

Pipeline operations:

### Continue

Calls the real implementation.

```csharp
x.Continue();
```

### Suppress

Does not call the real implementation. Returns default value for the method return type.

```csharp
x.Suppress();
```

### Return

Returns a specific value.

```csharp
x.Return(new PaymentResult(false));
```

### ReturnFixture

Uses `IAnvilData` to create a value compatible with the method return type.

```csharp
x.ReturnFixture();
```

### Capture

Records invocation information.

```csharp
x.Capture();
```

### Throw

Throws an exception.

```csharp
x.Throw(new TimeoutException());
```

### Delay

Adds artificial latency.

```csharp
x.Delay(TimeSpan.FromSeconds(2));
```

### Use

Custom pipeline delegate.

```csharp
x.Use(async (ctx, next) => await next(ctx));
```

## 9. Capture Store

The capture store gives tests a consistent place to assert against observed behavior.

```csharp
public interface IAnvilCaptureStore
{
    void Add(AnvilCapturedInvocation invocation);
    IReadOnlyList<AnvilCapturedInvocation> Invocations { get; }
    IReadOnlyList<T> OfType<T>();
}
```

```csharp
public sealed record AnvilCapturedInvocation(
    Type ServiceType,
    MethodInfo Method,
    object?[] Arguments,
    object? ReturnValue,
    Exception? Exception,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt);
```

Convenience helpers:

```csharp
app.Captured.InvocationsFor<IMessageBus>();
app.Captured.ArgumentsOfType<ReservationCreated>();
app.Captured.ReturnsOfType<PaymentResult>();
```

## 10. Scenario Model

Scenarios are reusable test configuration modules.

```csharp
public interface IAnvilScenario
{
    void Configure(IAnvilTestApplicationBuilder builder);
}
```

Example:

```csharp
public sealed class PaymentDeclinedScenario : IAnvilScenario
{
    public void Configure(IAnvilTestApplicationBuilder builder)
    {
        builder.Intercept<IPaymentGateway>(x => x
            .Return(new PaymentResult(false, "Declined")));
    }
}
```

Usage:

```csharp
await using var app = await AnvilTestApplication
    .CreateBuilder<Program>()
    .ApplyScenario<PaymentDeclinedScenario>()
    .BuildAsync();
```

Scenarios should be composable:

```csharp
builder
    .ApplyScenario<PaymentDeclinedScenario>()
    .ApplyScenario<CaptureMessagesScenario>()
    .ApplyScenario<PostgresScenario>();
```

## 11. xUnit Integration

xUnit support should make lifecycle usage ergonomic without replacing xUnit.

Option 1: inline usage.

```csharp
public sealed class ReservationTests
{
    [Fact]
    public async Task CreateReservation_returns_created()
    {
        await using var app = await AnvilTestApplication
            .CreateBuilder<Program>()
            .BuildAsync();
    }
}
```

Option 2: reusable fixture.

```csharp
public sealed class ReservationTestFixture : AnvilXunitFixture<Program>
{
    protected override void Configure(IAnvilTestApplicationBuilder builder)
    {
        builder.AddPostgres("db");
        builder.Intercept<IMessageBus>(x => x.Capture().Suppress());
    }
}
```

```csharp
public sealed class ReservationTests : IClassFixture<ReservationTestFixture>
{
    private readonly ReservationTestFixture _fixture;

    public ReservationTests(ReservationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateReservation_returns_created()
    {
        var client = _fixture.Application.CreateClient();
    }
}
```

## 12. Example Developer Experience

### API Integration Test

```csharp
public sealed class ReservationApiTests
{
    [Fact]
    public async Task CreateReservation_publishes_event()
    {
        // Arrange
        await using var app = await AnvilTestApplication
            .CreateBuilder<Program>()
            .AddPostgres("db")
            .Intercept<IMessageBus>(x => x.Capture().Suppress())
            .BuildAsync();

        var client = app.CreateClient();
        var request = app.Data.Create<CreateReservationRequest>();

        // Act
        var response = await client.PostAsJsonAsync("/reservations", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        app.Captured.ArgumentsOfType<ReservationCreated>().Count().ShouldBe(1);
    }
}
```

### Component Test

```csharp
public sealed class ReservationServiceTests
{
    [Fact]
    public async Task CreateAsync_returns_reservation_when_payment_succeeds()
    {
        // Arrange
        await using var app = await AnvilTestApplication
            .CreateBuilder()
            .ConfigureServices(services =>
            {
                services.AddReservationServices();
            })
            .Intercept<IPaymentGateway>(x => x.ReturnFixture())
            .Intercept<IMessageBus>(x => x.Capture().Suppress())
            .BuildAsync();

        var service = app.Services.GetRequiredService<IReservationService>();
        var request = app.Data.Create<CreateReservationRequest>();

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.ShouldNotBeNull();
        app.Captured.ArgumentsOfType<ReservationCreated>().Single().ShouldNotBeNull();
    }
}
```

## 13. Configuration Injection

Infrastructure resources should expose configuration into the application host.

Example:

```csharp
builder.AddPostgres("db", options =>
{
    options.ConfigurationKey = "ConnectionStrings:Default";
});
```

The resource should inject:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=...;Database=..."
  }
}
```

## 14. Logging and Diagnostics

v1 should include simple diagnostics:

1. List resources started.
2. List configured interceptors.
3. Capture intercepted calls.
4. Capture exceptions thrown inside interceptor pipeline.
5. Optional xUnit output integration.

Example:

```csharp
builder.UseXUnit(output);
```

Later versions can add OpenTelemetry tracing.

## 15. Error Handling Rules

AnvilCore should fail fast with useful exceptions.

Examples:

1. Attempting to intercept a concrete class without support.
2. Attempting to return fixture data for `void` without suppress semantics.
3. Attempting to add two resources with the same name.
4. Attempting to build without required provider modules.
5. Attempting to intercept a service not registered in DI, unless explicit substitute mode is enabled.

Exception style:

```text
AnvilInterceptionException: Service IMessageBus could not be intercepted because no registration was found. Register the service first or call AddSubstitute<IMessageBus>().
```

## 16. Initial Implementation Plan

### Milestone 1: Repository Skeleton

Create solution and projects.

```powershell
dotnet new sln -n AnvilCore
mkdir src tests samples

dotnet new classlib -n AnvilCore -o src/AnvilCore -f net10.0
dotnet new classlib -n AnvilCore.Interception -o src/AnvilCore.Interception -f net10.0
dotnet new classlib -n AnvilCore.AspNetCore -o src/AnvilCore.AspNetCore -f net10.0
dotnet new classlib -n AnvilCore.Xunit -o src/AnvilCore.Xunit -f net10.0
dotnet new classlib -n AnvilCore.AutoFixture -o src/AnvilCore.AutoFixture -f net10.0
dotnet new classlib -n AnvilCore.NSubstitute -o src/AnvilCore.NSubstitute -f net10.0
dotnet new classlib -n AnvilCore.Testcontainers -o src/AnvilCore.Testcontainers -f net10.0

dotnet sln add src/**/*.csproj
```

### Milestone 2: Core Runtime

Implement:

1. `IAnvilTestApplication`
2. `IAnvilTestApplicationBuilder`
3. `IAnvilModule`
4. `IAnvilData`
5. `IAnvilCaptureStore`
6. Basic `AnvilTestApplicationBuilder`

### Milestone 3: Data Provider

Implement AutoFixture provider.

```csharp
builder.UseAutoFixture();
```

### Milestone 4: Substitute Provider

Implement NSubstitute provider.

```csharp
builder.UseNSubstitute();
```

### Milestone 5: Interception Engine

Implement interface proxy-based interception.

Recommended starting implementation choices:

1. Use `DispatchProxy` for v1 to avoid external dependency.
2. Keep an internal abstraction so Castle DynamicProxy or a generated proxy implementation can replace it later.
3. Restrict v1 interception to interface services.

Implement:

1. `AnvilInvocationContext`
2. `IAnvilInterceptor`
3. `AnvilNextDelegate`
4. `AnvilInterceptorPipeline`
5. `CaptureInterceptor`
6. `SuppressInterceptor`
7. `ReturnValueInterceptor`
8. `ReturnFixtureInterceptor`
9. `ContinueInterceptor`

### Milestone 6: ASP.NET Core Integration

Implement `WebApplicationFactory<TEntryPoint>` wrapper.

```csharp
await using var app = await AnvilTestApplication
    .CreateBuilder<Program>()
    .BuildAsync();

var client = app.CreateClient();
```

### Milestone 7: Testcontainers Integration

Implement base resource lifecycle.

Then add:

1. Postgres
2. Redis
3. RabbitMQ

Each resource must:

1. Start before the application host.
2. Inject configuration into the application.
3. Stop after the application host disposes.

### Milestone 8: Dogfood Sample

Create a sample API with:

1. Controller or minimal API endpoint.
2. Service.
3. Repository.
4. Message bus abstraction.
5. Payment gateway abstraction.

Write tests proving:

1. API can start with explicit infrastructure.
2. DI service can be intercepted.
3. Interceptor can continue.
4. Interceptor can suppress.
5. Interceptor can return fixture data.
6. Interceptor can capture invocations.

## 17. Recommended v1 API Surface

```csharp
await using var app = await AnvilTestApplication
    .CreateBuilder<Program>()
    .UseAutoFixture()
    .UseNSubstitute()
    .AddPostgres("db")
    .Intercept<IMessageBus>(x => x.Capture().Suppress())
    .BuildAsync();
```

```csharp
builder.Intercept<TService>(pipeline => pipeline
    .Capture()
    .ReturnFixture());
```

```csharp
builder.ApplyScenario<TScenario>();
```

```csharp
app.Data.Create<T>();
```

```csharp
app.Captured.InvocationsFor<TService>();
```

```csharp
app.CreateClient();
```

## 18. VS Code Development Setup

Recommended extensions:

1. C# Dev Kit
2. C#
3. NuGet Gallery
4. GitHub Pull Requests
5. Docker
6. Dev Containers, optional

Recommended root files:

```text
.editorconfig
global.json
Directory.Build.props
Directory.Packages.props
README.md
CHANGELOG.md
LICENSE
```

### global.json

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature"
  }
}
```

### Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest</AnalysisLevel>
  </PropertyGroup>
</Project>
```

## 19. Design Risks

### Risk 1: DI Interception Complexity

ASP.NET Core DI does not have native interception. Decorating registrations must be carefully implemented.

Mitigation:

1. Start with interface-only services.
2. Preserve lifetimes.
3. Dogfood heavily.
4. Add tests around singleton/scoped/transient behavior.

### Risk 2: DispatchProxy Limitations

`DispatchProxy` is simple but may have performance and ergonomics limitations.

Mitigation:

Keep proxy creation behind:

```csharp
public interface IAnvilProxyFactory
{
    object CreateProxy(Type serviceType, object inner, AnvilInterceptorPipeline pipeline);
}
```

### Risk 3: Over-Abstraction

Provider abstractions can become too abstract too early.

Mitigation:

Only abstract the tools you explicitly want to replace:

1. Data generation.
2. Substitute creation.
3. Proxy creation.
4. Infrastructure resources.

Do not abstract xUnit deeply in v1.

### Risk 4: Testcontainers Startup Time

Starting infrastructure per test can be slow.

Mitigation:

1. Support class fixtures.
2. Support collection fixtures.
3. Allow resource reuse later.
4. Make per-test and shared-lifetime usage explicit.

## 20. Future Roadmap

### v1

1. xUnit only.
2. DI-only interface interception.
3. Explicit infrastructure resources.
4. AutoFixture provider.
5. NSubstitute provider.
6. ASP.NET Core WebApplicationFactory support.
7. Capture store.
8. Scenario modules.

### v2

1. Source generators for strongly typed scenarios.
2. Generated fixture builders.
3. OpenTelemetry traces.
4. HTTP client interception.
5. Shared resource lifetimes.
6. More Testcontainers modules.
7. FluentAssertions adapter.
8. Moq adapter.

### v3

1. Visual test topology output.
2. Scenario reports.
3. Test execution traces.
4. Generated diagrams.
5. Contract testing support.
6. Message broker-level capture.
7. Distributed application testing with multiple projects.

## 21. Open Questions for Later

These do not block v1, but should be answered before v2.

1. Should AnvilCore eventually support Aspire AppHost projects directly?
2. Should AnvilCore support source-generated proxies?
3. Should scenario definitions be attributes, classes, or fluent-only?
4. Should AnvilCore provide opinionated Shouldly helpers?
5. Should infrastructure resources support reuse across test classes?
6. Should captured invocations be exported as JSON for debugging?
7. Should interception rules support conditional matching by method name or arguments?

## 22. Recommended First Build Slice

The first working slice should be intentionally small:

```csharp
[Fact]
public async Task Can_intercept_message_bus_and_capture_published_message()
{
    await using var app = await AnvilTestApplication
        .CreateBuilder<Program>()
        .UseAutoFixture()
        .Intercept<IMessageBus>(x => x.Capture().Suppress())
        .BuildAsync();

    var client = app.CreateClient();
    var request = app.Data.Create<CreateReservationRequest>();

    var response = await client.PostAsJsonAsync("/reservations", request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    app.Captured.ArgumentsOfType<ReservationCreated>().Count().ShouldBe(1);
}
```

This proves the core value:

1. Application starts normally.
2. Test data is generated.
3. DI service is intercepted.
4. Real implementation is suppressed.
5. Call is captured.
6. Test uses normal Arrange, Act, Assert.

## 23. Final Positioning

AnvilCore should be described as:

> An Aspire-inspired testing platform for .NET that composes test infrastructure and intercepts dependency-injection services through a middleware-style pipeline, allowing application developers to write normal xUnit tests while selectively observing, replacing, suppressing, or continuing application behavior.

