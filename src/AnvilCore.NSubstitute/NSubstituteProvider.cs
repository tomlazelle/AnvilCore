using NSubstitute;

namespace AnvilCore.NSubstitute;

public sealed class NSubstituteProvider : IAnvilSubstituteProvider
{
    public object Create(Type serviceType) =>
        Substitute.For([serviceType], []);

    public T Create<T>() where T : class =>
        Substitute.For<T>();
}
