using AnvilCore.Testcontainers;

namespace AnvilCore.Testcontainers.RabbitMq;

public static class AnvilBuilderRabbitMqExtensions
{
    public static IAnvilTestApplicationBuilder AddRabbitMq(
        this IAnvilTestApplicationBuilder builder,
        string name,
        string? configurationKey = null)
    {
        var key = configurationKey ?? $"ConnectionStrings:{name}";
        return builder.AddResource(new AnvilRabbitMqResource(name, key));
    }
}
