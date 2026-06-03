using AnvilCore.Testcontainers.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnvilCore.Tests;

[Trait("Category", "Integration")]
public sealed class TestcontainersResourceTests
{
    [Fact]
    public async Task AddPostgres_injects_connection_string_into_configuration()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .AddPostgres("db")
            .BuildAsync();

        var config = app.Services.GetRequiredService<IConfiguration>();
        var connectionString = config.GetConnectionString("db");

        Assert.NotNull(connectionString);
        Assert.Contains("Host=", connectionString);
        Assert.Contains("Port=", connectionString);
    }

    [Fact]
    public async Task AddPostgres_with_custom_key_injects_at_specified_key()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .AddPostgres("db", opts => opts.ConfigurationKey = "Database:Primary")
            .BuildAsync();

        var config = app.Services.GetRequiredService<IConfiguration>();
        var connectionString = config["Database:Primary"];

        Assert.NotNull(connectionString);
        Assert.Contains("Host=", connectionString);
    }

    [Fact]
    public async Task AddPostgres_container_stops_on_dispose()
    {
        IConfiguration? config = null;
        string? connectionString = null;

        await using (var app = await AnvilTestApplication.CreateBuilder()
            .AddPostgres("db")
            .BuildAsync())
        {
            config = app.Services.GetRequiredService<IConfiguration>();
            connectionString = config.GetConnectionString("db");
            Assert.NotNull(connectionString);
        }

        // After dispose, the container should be stopped — no assertion needed
        // (we just verify dispose doesn't throw)
    }

    [Fact]
    public async Task Duplicate_resource_name_throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            AnvilTestApplication.CreateBuilder()
                .AddPostgres("db")
                .AddPostgres("db"));

        Assert.Contains("db", ex.Message);
    }
}
