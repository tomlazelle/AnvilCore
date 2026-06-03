namespace AnvilCore;

internal sealed class NullAnvilData : IAnvilData
{
    private static InvalidOperationException NoProvider() => new(
        "No data provider is registered. Call UseAutoFixture() or register an IAnvilData implementation.");

    public object Create(Type type) => throw NoProvider();
    public T Create<T>() => throw NoProvider();
    public IEnumerable<T> CreateMany<T>(int count = 3) => throw NoProvider();
}
