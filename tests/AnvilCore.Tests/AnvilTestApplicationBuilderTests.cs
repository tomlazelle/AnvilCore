using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.Tests;

public sealed class AnvilTestApplicationBuilderTests
{
    [Fact]
    public async Task BuildAsync_resolves_registered_service()
    {
        var builder = AnvilTestApplication.CreateBuilder();
        builder.ConfigureServices(s => s.AddSingleton<ITestService, TestService>());

        await using var app = await builder.BuildAsync();

        Assert.NotNull(app.Services.GetRequiredService<ITestService>());
    }

    [Fact]
    public async Task BuildAsync_provides_capture_store()
    {
        await using var app = await AnvilTestApplication.CreateBuilder().BuildAsync();

        Assert.NotNull(app.Captured);
        Assert.Empty(app.Captured.Invocations);
    }

    [Fact]
    public async Task BuildAsync_provides_services_provider()
    {
        await using var app = await AnvilTestApplication.CreateBuilder().BuildAsync();

        Assert.NotNull(app.Services);
    }

    [Fact]
    public async Task ApplyScenario_generic_configures_builder()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .ApplyScenario<RegisterTestServiceScenario>()
            .BuildAsync();

        Assert.NotNull(app.Services.GetRequiredService<ITestService>());
    }

    [Fact]
    public async Task ApplyScenario_instance_configures_builder()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .ApplyScenario(new RegisterTestServiceScenario())
            .BuildAsync();

        Assert.NotNull(app.Services.GetRequiredService<ITestService>());
    }

    [Fact]
    public async Task Scenarios_are_composable()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .ApplyScenario<RegisterTestServiceScenario>()
            .ApplyScenario<RegisterOtherServiceScenario>()
            .BuildAsync();

        Assert.NotNull(app.Services.GetRequiredService<ITestService>());
        Assert.NotNull(app.Services.GetRequiredService<IOtherService>());
    }

    [Fact]
    public async Task AddModule_configures_builder()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .AddModule(new TestModule())
            .BuildAsync();

        Assert.NotNull(app.Services.GetRequiredService<ITestService>());
    }

    [Fact]
    public async Task Data_throws_when_no_provider_registered()
    {
        await using var app = await AnvilTestApplication.CreateBuilder().BuildAsync();

        Assert.Throws<InvalidOperationException>(() => app.Data.Create<string>());
    }

    private interface ITestService { }
    private sealed class TestService : ITestService { }

    private interface IOtherService { }
    private sealed class OtherService : IOtherService { }

    private sealed class RegisterTestServiceScenario : IAnvilScenario
    {
        public void Configure(IAnvilTestApplicationBuilder builder) =>
            builder.ConfigureServices(s => s.AddSingleton<ITestService, TestService>());
    }

    private sealed class RegisterOtherServiceScenario : IAnvilScenario
    {
        public void Configure(IAnvilTestApplicationBuilder builder) =>
            builder.ConfigureServices(s => s.AddSingleton<IOtherService, OtherService>());
    }

    private sealed class TestModule : IAnvilModule
    {
        public void Configure(IAnvilTestApplicationBuilder builder) =>
            builder.ConfigureServices(s => s.AddSingleton<ITestService, TestService>());
    }
}
