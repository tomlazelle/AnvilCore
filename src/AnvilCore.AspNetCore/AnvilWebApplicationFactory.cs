using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.AspNetCore;

internal sealed class AnvilWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly IAnvilTestApplicationBuilder _builder;

    internal AnvilWebApplicationFactory(IAnvilTestApplicationBuilder builder)
    {
        _builder = builder;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        // Inject resource config (connection strings from Testcontainers, etc.) via ConfigureAppConfiguration
        // so it MERGES with the app's existing configuration rather than replacing it.
        if (_builder.Properties.TryGetValue(AnvilBuilderProperties.ResourceConfig, out var rcObj) &&
            rcObj is Dictionary<string, string?> resourceConfig && resourceConfig.Count > 0)
        {
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(resourceConfig));
        }

        builder.ConfigureServices(services =>
        {
            // Inject builder services (IAnvilData, IAnvilCaptureStore, test overrides, etc.)
            foreach (var descriptor in _builder.Services)
                services.Add(descriptor);

            // Run post-configure actions (interceptor decorations) on the merged collection.
            if (_builder.Properties.TryGetValue(AnvilBuilderProperties.PostConfigureActions, out var paObj) &&
                paObj is List<Action<IServiceCollection>> actions)
            {
                foreach (var action in actions)
                    action(services);
            }
        });
    }
}
