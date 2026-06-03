using AnvilCore.Testcontainers;

namespace AnvilCore.Testcontainers.Redis;

public static class AnvilBuilderRedisExtensions
{
    public static IAnvilTestApplicationBuilder AddRedis(
        this IAnvilTestApplicationBuilder builder,
        string name,
        string? configurationKey = null)
    {
        var key = configurationKey ?? $"ConnectionStrings:{name}";
        return builder.AddResource(new AnvilRedisResource(name, key));
    }
}
