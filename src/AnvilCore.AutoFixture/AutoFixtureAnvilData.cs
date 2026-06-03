using AutoFixture;
using AutoFixture.Kernel;

namespace AnvilCore.AutoFixture;

public sealed class AutoFixtureAnvilData : IAnvilData
{
    private readonly IFixture _fixture;

    public AutoFixtureAnvilData() : this(new Fixture()) { }

    public AutoFixtureAnvilData(IFixture fixture)
    {
        _fixture = fixture;
    }

    public object Create(Type type)
    {
        var context = new SpecimenContext(_fixture);
        return context.Resolve(type);
    }

    public T Create<T>() => _fixture.Create<T>();

    public IEnumerable<T> CreateMany<T>(int count = 3) => _fixture.CreateMany<T>(count);
}
