using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.Testcontainers;

public static class AnvilResourceExtensions
{
    private const string ResourceNamesKey = "anvil:resource-names";

    public static IAnvilTestApplicationBuilder AddResource(
        this IAnvilTestApplicationBuilder builder,
        IAnvilResource resource)
    {
        if (!builder.Properties.TryGetValue(ResourceNamesKey, out var namesObj) ||
            namesObj is not HashSet<string> names)
        {
            names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            builder.Properties[ResourceNamesKey] = names;
        }

        if (!names.Add(resource.Name))
            throw new InvalidOperationException(
                $"A resource named '{resource.Name}' is already registered.");

        // Pre-build: start the container and populate resource config
        AddPreBuildAction(builder, async ct =>
        {
            await resource.StartAsync(ct);
            AnvilBuilderProperties.Log(builder.Properties,
                $"[AnvilCore] Started resource '{resource.Name}' ({resource.GetType().Name})");

            // Extract config entries so both plain-DI and web-host paths can use them
            var tempConfig = new ConfigurationBuilder();
            resource.ConfigureApplication(tempConfig);
            var built = tempConfig.Build();

            var entries = built.AsEnumerable()
                .Where(kv => kv.Value is not null)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            // Store for web-host ConfigureAppConfiguration merge
            if (!builder.Properties.TryGetValue(AnvilBuilderProperties.ResourceConfig, out var rcObj) ||
                rcObj is not Dictionary<string, string?> resourceConfig)
            {
                resourceConfig = new Dictionary<string, string?>();
                builder.Properties[AnvilBuilderProperties.ResourceConfig] = resourceConfig;
            }
            foreach (var (key, value) in entries)
                resourceConfig[key] = value;

            // Register IConfiguration for plain-DI path (merged with any prior IConfiguration)
            MergeConfiguration(builder.Services, entries);

            // Apply resource-specific service registrations
            resource.ConfigureServices(builder.Services);
        });

        // App-dispose: stop and dispose the container
        AddDisposeCallback(builder, async () =>
        {
            await resource.StopAsync();
            await resource.DisposeAsync();
        });

        return builder;
    }

    private static void AddPreBuildAction(IAnvilTestApplicationBuilder builder, Func<CancellationToken, ValueTask> action)
    {
        if (!builder.Properties.TryGetValue(AnvilBuilderProperties.PreBuildAsyncActions, out var obj) ||
            obj is not List<Func<CancellationToken, ValueTask>>)
        {
            obj = new List<Func<CancellationToken, ValueTask>>();
            builder.Properties[AnvilBuilderProperties.PreBuildAsyncActions] = obj;
        }
        ((List<Func<CancellationToken, ValueTask>>)obj).Add(action);
    }

    private static void AddDisposeCallback(IAnvilTestApplicationBuilder builder, Func<ValueTask> callback)
    {
        if (!builder.Properties.TryGetValue(AnvilBuilderProperties.AppDisposeCallbacks, out var obj) ||
            obj is not List<Func<ValueTask>>)
        {
            obj = new List<Func<ValueTask>>();
            builder.Properties[AnvilBuilderProperties.AppDisposeCallbacks] = obj;
        }
        ((List<Func<ValueTask>>)obj).Add(callback);
    }

    private static void MergeConfiguration(IServiceCollection services, IDictionary<string, string?> entries)
    {
        if (entries.Count == 0)
            return;

        var existing = services.FirstOrDefault(s => s.ServiceType == typeof(IConfiguration));
        IConfiguration merged;

        if (existing?.ImplementationInstance is IConfiguration existingConfig)
        {
            merged = new ConfigurationBuilder()
                .AddConfiguration(existingConfig)
                .AddInMemoryCollection(entries)
                .Build();
            services.Remove(existing);
        }
        else
        {
            merged = new ConfigurationBuilder()
                .AddInMemoryCollection(entries)
                .Build();
        }

        services.AddSingleton<IConfiguration>(merged);
    }
}
