using AnvilCore.AutoFixture;
using AnvilCore.Interception;
using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.Interception.Tests;

public sealed class InterceptionPipelineTests
{
    // ── Capture + Suppress ──────────────────────────────────────────────────

    [Fact]
    public async Task Capture_Suppress_records_invocation_and_suppresses_call()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IMessageBus, RealMessageBus>())
            .Intercept<IMessageBus>(x =>
            {
                x.Capture();
                x.Suppress();
            })
            .BuildAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        bus.Publish(new OrderPlaced("order-1"));

        Assert.Single(app.Captured.InvocationsFor<IMessageBus>());
        Assert.False(RealMessageBus.WasCalled);
    }

    // ── Capture + Continue ─────────────────────────────────────────────────

    [Fact]
    public async Task Capture_Continue_records_invocation_and_calls_real_implementation()
    {
        RealMessageBus.Reset();

        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IMessageBus, RealMessageBus>())
            .Intercept<IMessageBus>(x =>
            {
                x.Capture();
                x.Continue();
            })
            .BuildAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        bus.Publish(new OrderPlaced("order-2"));

        Assert.Single(app.Captured.InvocationsFor<IMessageBus>());
        Assert.True(RealMessageBus.WasCalled);
    }

    // ── Return fixed value ─────────────────────────────────────────────────

    [Fact]
    public async Task Return_returns_specified_value_without_calling_real_implementation()
    {
        var expected = new PaymentResult(false, "Blocked");

        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IPaymentGateway, RealPaymentGateway>())
            .Intercept<IPaymentGateway>(x => x.Return(expected))
            .BuildAsync();

        var gateway = app.Services.GetRequiredService<IPaymentGateway>();
        var result = gateway.Charge(100m);

        Assert.Equal(expected, result);
        Assert.False(RealPaymentGateway.WasCalled);
    }

    // ── ReturnFixture ──────────────────────────────────────────────────────

    [Fact]
    public async Task ReturnFixture_returns_generated_value()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IPaymentGateway, RealPaymentGateway>())
            .Intercept<IPaymentGateway>(x => x.ReturnFixture())
            .BuildAsync();

        var gateway = app.Services.GetRequiredService<IPaymentGateway>();
        var result = gateway.Charge(100m);

        Assert.NotNull(result);
    }

    // ── Throw ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Throw_throws_specified_exception()
    {
        var expectedException = new TimeoutException("gateway unavailable");

        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IPaymentGateway, RealPaymentGateway>())
            .Intercept<IPaymentGateway>(x => x.Throw(expectedException))
            .BuildAsync();

        var gateway = app.Services.GetRequiredService<IPaymentGateway>();

        Assert.Throws<TimeoutException>(() => gateway.Charge(50m));
    }

    // ── Messages (captured arguments) ─────────────────────────────────────

    [Fact]
    public async Task Captured_Messages_contains_published_arguments()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IMessageBus, RealMessageBus>())
            .Intercept<IMessageBus>(x => { x.Capture(); x.Suppress(); })
            .BuildAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        var message = new OrderPlaced("order-3");
        bus.Publish(message);

        Assert.Contains(message, app.Captured.Messages);
        Assert.Contains(message, app.Captured.ArgumentsOfType<OrderPlaced>());
    }

    // ── Use (custom delegate) ──────────────────────────────────────────────

    [Fact]
    public async Task Use_custom_delegate_runs_in_pipeline()
    {
        var called = false;

        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IMessageBus, RealMessageBus>())
            .Intercept<IMessageBus>(x =>
            {
                x.Use((ctx, next, ct) => { called = true; return next(ctx); });
                x.Suppress();
            })
            .BuildAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        bus.Publish(new OrderPlaced("order-4"));

        Assert.True(called);
    }

    // ── Non-interface intercept throws ─────────────────────────────────────

    [Fact]
    public void Intercept_concrete_class_throws_AnvilInterceptionException()
    {
        Assert.Throws<AnvilInterceptionException>(() =>
            AnvilTestApplication.CreateBuilder()
                .Intercept<RealMessageBus>(x => x.Suppress()));
    }

    // ── Service not registered throws ──────────────────────────────────────

    [Fact]
    public async Task Intercept_unregistered_service_throws_at_build_time()
    {
        await Assert.ThrowsAsync<AnvilInterceptionException>(() =>
            AnvilTestApplication.CreateBuilder()
                .Intercept<IMessageBus>(x => x.Suppress())
                .BuildAsync()
                .AsTask());
    }

    // ── Async return type ──────────────────────────────────────────────────

    [Fact]
    public async Task Intercept_async_method_capture_and_suppress()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IAsyncRepository, RealAsyncRepository>())
            .Intercept<IAsyncRepository>(x => { x.Capture(); x.Suppress(); })
            .BuildAsync();

        var repo = app.Services.GetRequiredService<IAsyncRepository>();
        var result = await repo.FindAsync("id-1");

        Assert.Null(result);
        Assert.Single(app.Captured.InvocationsFor<IAsyncRepository>());
    }

    [Fact]
    public async Task Intercept_async_method_continue_calls_real_implementation()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<IAsyncRepository, RealAsyncRepository>())
            .Intercept<IAsyncRepository>(x => { x.Capture(); x.Continue(); })
            .BuildAsync();

        var repo = app.Services.GetRequiredService<IAsyncRepository>();
        var result = await repo.FindAsync("id-1");

        Assert.NotNull(result);
        Assert.Equal("id-1", result.Id);
    }
}

// ── Test doubles ────────────────────────────────────────────────────────────

public interface IMessageBus
{
    void Publish(object message);
}

public interface IPaymentGateway
{
    PaymentResult Charge(decimal amount);
}

public interface IAsyncRepository
{
    Task<RepositoryItem?> FindAsync(string id);
}

public sealed record OrderPlaced(string OrderId);
public sealed record PaymentResult(bool Success, string? Reason = null);
public sealed record RepositoryItem(string Id);

public sealed class RealMessageBus : IMessageBus
{
    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;
    public void Publish(object message) => WasCalled = true;
}

public sealed class RealPaymentGateway : IPaymentGateway
{
    public static bool WasCalled { get; private set; }
    public PaymentResult Charge(decimal amount) { WasCalled = true; return new PaymentResult(true); }
}

public sealed class RealAsyncRepository : IAsyncRepository
{
    public Task<RepositoryItem?> FindAsync(string id) =>
        Task.FromResult<RepositoryItem?>(new RepositoryItem(id));
}
