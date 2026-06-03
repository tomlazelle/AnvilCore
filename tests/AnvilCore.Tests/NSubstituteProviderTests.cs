using AnvilCore.NSubstitute;
using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.Tests;

internal interface ITestService
{
    string GetValue();
}

public sealed class NSubstituteProviderTests
{
    [Fact]
    public async Task UseNSubstitute_registers_substitute_provider()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseNSubstitute()
            .BuildAsync();

        Assert.NotNull(app.Services.GetRequiredService<IAnvilSubstituteProvider>());
    }

    [Fact]
    public void NSubstituteProvider_creates_substitute_for_interface()
    {
        var provider = new NSubstituteProvider();

        var substitute = provider.Create<ITestService>();

        Assert.NotNull(substitute);
        Assert.IsAssignableFrom<ITestService>(substitute);
    }

    [Fact]
    public void NSubstituteProvider_creates_substitute_by_type()
    {
        var provider = new NSubstituteProvider();

        var substitute = provider.Create(typeof(ITestService));

        Assert.NotNull(substitute);
        Assert.IsAssignableFrom<ITestService>(substitute);
    }

    [Fact]
    public async Task AddSubstitute_registers_substitute_in_di()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseNSubstitute()
            .AddSubstitute<ITestService>()
            .BuildAsync();

        var service = app.Services.GetRequiredService<ITestService>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<ITestService>(service);
    }

    [Fact]
    public async Task AddSubstitute_without_UseNSubstitute_throws_descriptive_error()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .AddSubstitute<ITestService>()
            .BuildAsync();

        var ex = Assert.Throws<InvalidOperationException>(
            () => app.Services.GetRequiredService<ITestService>());

        Assert.Contains("UseNSubstitute", ex.Message);
    }

}
