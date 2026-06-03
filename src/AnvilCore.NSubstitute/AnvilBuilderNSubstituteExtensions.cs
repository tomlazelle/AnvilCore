using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.NSubstitute;

public static class AnvilBuilderNSubstituteExtensions
{
    public static IAnvilTestApplicationBuilder UseNSubstitute(
        this IAnvilTestApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IAnvilSubstituteProvider, NSubstituteProvider>();
        return builder;
    }

    public static IAnvilTestApplicationBuilder AddSubstitute<TService>(
        this IAnvilTestApplicationBuilder builder)
        where TService : class
    {
        builder.Services.AddSingleton(typeof(TService), sp =>
        {
            var provider = sp.GetService<IAnvilSubstituteProvider>()
                ?? throw new InvalidOperationException(
                    $"No substitute provider is registered. Call UseNSubstitute() before AddSubstitute<{typeof(TService).Name}>().");
            return provider.Create(typeof(TService));
        });
        return builder;
    }
}
