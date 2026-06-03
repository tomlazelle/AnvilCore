using System.Net;
using System.Net.Http.Json;
using AnvilCore.AspNetCore;
using AnvilCore.AutoFixture;
using AnvilCore.Interception;
using AnvilCore.SampleApi;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AnvilCore.SampleApi.Tests;

public sealed class ReservationApiTests
{
    // ── 1. API starts with explicit infrastructure ─────────────────────────

    [Fact]
    public async Task Api_starts_and_accepts_requests()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .UseAutoFixture()
            .BuildAsync();

        var client = app.CreateClient();
        var request = app.Data.Create<CreateReservationRequest>();

        var response = await client.PostAsJsonAsync("/reservations", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    // ── 2. DI service can be intercepted ──────────────────────────────────

    [Fact]
    public async Task Intercept_registers_proxy_for_service()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .UseAutoFixture()
            .Intercept<IMessageBus>(x => x.Suppress())
            .BuildAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();

        bus.ShouldNotBeNull();
        bus.ShouldNotBeOfType<NoOpMessageBus>(); // proxy, not the real type
    }

    // ── 3. Interceptor can continue ────────────────────────────────────────

    [Fact]
    public async Task Interceptor_continue_calls_real_implementation()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .UseAutoFixture()
            .Intercept<IMessageBus>(x => { x.Capture(); x.Continue(); })
            .BuildAsync();

        var client = app.CreateClient();
        var request = app.Data.Create<CreateReservationRequest>();

        var response = await client.PostAsJsonAsync("/reservations", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        // CaptureInterceptor records the invocation; ContinueInterceptor calls the real NoOpMessageBus
        app.Captured.InvocationsFor<IMessageBus>().Count.ShouldBe(1);
    }

    // ── 4. Interceptor can suppress ────────────────────────────────────────

    [Fact]
    public async Task Interceptor_suppress_blocks_payment_and_returns_bad_request()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .UseAutoFixture()
            .Intercept<IPaymentGateway>(x => x.Return(new PaymentResult(false, "Suppressed")))
            .BuildAsync();

        var client = app.CreateClient();
        var request = app.Data.Create<CreateReservationRequest>();

        var response = await client.PostAsJsonAsync("/reservations", request);

        response.StatusCode.ShouldBe(HttpStatusCode.PaymentRequired);
    }

    // ── 5. Interceptor can return fixture data ─────────────────────────────

    [Fact]
    public async Task Interceptor_return_fixture_provides_generated_payment_result()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .ConfigureServices(s =>
            {
                s.AddSingleton<IPaymentGateway, AlwaysSucceedPaymentGateway>();
            })
            .Intercept<IPaymentGateway>(x => { x.Capture(); x.ReturnFixture(); })
            .BuildAsync();

        var gateway = app.Services.GetRequiredService<IPaymentGateway>();
        var result = await gateway.ChargeAsync(100m);

        result.ShouldNotBeNull();
        app.Captured.InvocationsFor<IPaymentGateway>().Count.ShouldBe(1);
        app.Captured.ReturnsOfType<PaymentResult>().Count.ShouldBe(1);
    }

    // ── 6. Interceptor can capture invocations ─────────────────────────────

    [Fact]
    public async Task Interceptor_capture_records_published_reservation_created()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .UseAutoFixture()
            .Intercept<IMessageBus>(x => { x.Capture(); x.Suppress(); })
            .BuildAsync();

        var client = app.CreateClient();
        var request = app.Data.Create<CreateReservationRequest>();

        var response = await client.PostAsJsonAsync("/reservations", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        app.Captured.ArgumentsOfType<ReservationCreated>().Count.ShouldBe(1);
    }

}
