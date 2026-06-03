using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.AutoFixture;

public static class AnvilBuilderAutoFixtureExtensions
{
    public static IAnvilTestApplicationBuilder UseAutoFixture(
        this IAnvilTestApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IAnvilData, AutoFixtureAnvilData>();
        return builder;
    }
}
