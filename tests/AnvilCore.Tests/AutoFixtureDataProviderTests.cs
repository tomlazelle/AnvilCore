using AnvilCore.AutoFixture;

namespace AnvilCore.Tests;

public sealed class AutoFixtureDataProviderTests
{
    [Fact]
    public async Task UseAutoFixture_creates_typed_instance()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .BuildAsync();

        var result = app.Data.Create<SampleRecord>();

        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.False(string.IsNullOrEmpty(result.Name));
    }

    [Fact]
    public async Task UseAutoFixture_creates_instance_by_type()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .BuildAsync();

        var result = app.Data.Create(typeof(SampleRecord));

        Assert.NotNull(result);
        Assert.IsType<SampleRecord>(result);
    }

    [Fact]
    public async Task UseAutoFixture_creates_many()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .BuildAsync();

        var results = app.Data.CreateMany<SampleRecord>();

        Assert.Equal(3, results.Count());
        Assert.All(results, r => Assert.NotNull(r));
    }

    [Fact]
    public async Task UseAutoFixture_creates_many_with_custom_count()
    {
        await using var app = await AnvilTestApplication.CreateBuilder()
            .UseAutoFixture()
            .BuildAsync();

        var results = app.Data.CreateMany<SampleRecord>(5);

        Assert.Equal(5, results.Count());
    }

    [Fact]
    public async Task Without_provider_Data_throws_InvalidOperationException()
    {
        await using var app = await AnvilTestApplication.CreateBuilder().BuildAsync();

        var ex = Assert.Throws<InvalidOperationException>(() => app.Data.Create<SampleRecord>());
        Assert.Contains("UseAutoFixture", ex.Message);
    }

    private sealed record SampleRecord(int Id, string Name);
}
