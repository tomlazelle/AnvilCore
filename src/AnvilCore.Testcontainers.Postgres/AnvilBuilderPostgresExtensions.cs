using AnvilCore.Testcontainers;

namespace AnvilCore.Testcontainers.Postgres;

public static class AnvilBuilderPostgresExtensions
{
    public static IAnvilTestApplicationBuilder AddPostgres(
        this IAnvilTestApplicationBuilder builder,
        string name,
        Action<AnvilPostgresOptions>? configure = null)
    {
        var options = new AnvilPostgresOptions
        {
            ConfigurationKey = $"ConnectionStrings:{name}"
        };
        configure?.Invoke(options);

        return builder.AddResource(new AnvilPostgresResource(name, options));
    }
}
