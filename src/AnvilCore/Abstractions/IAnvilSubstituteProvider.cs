namespace AnvilCore;

public interface IAnvilSubstituteProvider
{
    object Create(Type serviceType);
    T Create<T>() where T : class;
}
