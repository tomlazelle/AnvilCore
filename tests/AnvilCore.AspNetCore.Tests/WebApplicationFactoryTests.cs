using System.Net;
using AnvilCore.AspNetCore;
using AnvilCore.AutoFixture;
using AnvilCore.Interception;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AnvilCore.AspNetCore.Tests;

public sealed class WebApplicationFactoryTests
{
    [Fact]
    public async Task App_starts_and_responds_to_http_requests()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .BuildAsync();

        var client = app.CreateClient();
        var response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Services_returns_web_app_service_provider()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .BuildAsync();

        app.Services.ShouldNotBeNull();
    }

    [Fact]
    public async Task Builder_services_are_available_in_web_app()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .ConfigureServices(s => s.AddSingleton<ITestMarker, TestMarker>())
            .BuildAsync();

        var marker = app.Services.GetRequiredService<ITestMarker>();
        marker.ShouldNotBeNull();
    }

    [Fact]
    public async Task UseAutoFixture_data_is_available_in_web_app()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .UseAutoFixture()
            .BuildAsync();

        var result = app.Data.Create<SampleDto>();

        result.ShouldNotBeNull();
        result.Name.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Capture_store_is_available_in_web_app()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .BuildAsync();

        app.Captured.ShouldNotBeNull();
        app.Captured.Invocations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Intercept_service_registered_via_builder_in_web_app()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<Program>()
            .UseAutoFixture()
            .ConfigureServices(s => s.AddSingleton<ITestMarker, TestMarker>())
            .Intercept<ITestMarker>(x => { x.Capture(); x.Suppress(); })
            .BuildAsync();

        var marker = app.Services.GetRequiredService<ITestMarker>();
        marker.Mark();

        app.Captured.InvocationsFor<ITestMarker>().Count.ShouldBe(1);
    }

    [Fact]
    public async Task CreateClient_throws_for_plain_di_app()
    {
        await using var app = await AnvilTestApplication.CreateBuilder().BuildAsync();

        Should.Throw<InvalidOperationException>(() => app.CreateClient());
    }

    private interface ITestMarker { void Mark(); }
    private sealed class TestMarker : ITestMarker { public void Mark() { } }
    private sealed record SampleDto(string Name, int Value);
}
