using System.Net;
using AnvilCore.AspNetCore;
using AnvilCore.Interception;
using AnvilCore.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace AnvilCore.AspNetCore.Tests;

// ── Shared fixture definition ────────────────────────────────────────────────

public sealed class SampleApiFixture : AnvilXunitFixture<Program>
{
    protected override void Configure(IAnvilTestApplicationBuilder builder) { }
}

// ── Tests using the shared fixture ──────────────────────────────────────────

public sealed class XunitFixtureTests : IClassFixture<SampleApiFixture>
{
    private readonly SampleApiFixture _fixture;

    public XunitFixtureTests(SampleApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task First_fact_uses_shared_app_instance()
    {
        var client = _fixture.Application.CreateClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Second_fact_uses_same_shared_app_instance()
    {
        // Both facts share the same SampleApiFixture instance — the app
        // is started once and disposed after the test class finishes.
        var client = _fixture.Application.CreateClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public void Fixture_application_is_not_null()
    {
        _fixture.Application.ShouldNotBeNull();
        _fixture.Application.Services.ShouldNotBeNull();
        _fixture.Application.Captured.ShouldNotBeNull();
    }
}

// ── UseXUnit logging tests ───────────────────────────────────────────────────

public sealed class UseXUnitTests
{
    [Fact]
    public async Task UseXUnit_routes_interceptor_messages_to_output()
    {
        var logged = new List<string>();
        var output = new CapturingOutputHelper(logged);

        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseXUnit(output)
            .ConfigureServices(s => s.AddSingleton<ILoggingService, RealLoggingService>())
            .Intercept<ILoggingService>(x => x.Capture().Suppress())
            .BuildAsync();

        logged.ShouldContain(m => m.Contains("ILoggingService"));
        app.Services.ShouldNotBeNull();
    }

    [Fact]
    public async Task UseXUnit_extension_does_not_throw()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .UseXUnit(new NoOpOutputHelper())
            .BuildAsync();

        app.ShouldNotBeNull();
    }

    private interface ILoggingService { void Log(string message); }
    private sealed class RealLoggingService : ILoggingService { public void Log(string message) { } }

    private sealed class NoOpOutputHelper : ITestOutputHelper
    {
        public void WriteLine(string message) { }
        public void WriteLine(string format, params object[] args) { }
    }

    private sealed class CapturingOutputHelper(List<string> messages) : ITestOutputHelper
    {
        public void WriteLine(string message) => messages.Add(message);
        public void WriteLine(string format, params object[] args) => messages.Add(string.Format(format, args));
    }
}
