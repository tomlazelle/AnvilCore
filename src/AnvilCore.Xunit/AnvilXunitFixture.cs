using AnvilCore.AspNetCore;
using Xunit;

namespace AnvilCore.Xunit;

public abstract class AnvilXunitFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private IAnvilTestApplication? _application;

    public IAnvilTestApplication Application => _application ??
        throw new InvalidOperationException(
            "Fixture has not been initialized. Ensure InitializeAsync() has run.");

    protected abstract void Configure(IAnvilTestApplicationBuilder builder);

    public async Task InitializeAsync()
    {
        var builder = AnvilTestApplication.CreateBuilder()
            .UseWebApplicationFactory<TEntryPoint>();

        Configure(builder);

        _application = await builder.BuildAsync();
    }

    public async Task DisposeAsync()
    {
        if (_application is not null)
            await _application.DisposeAsync();
    }
}
