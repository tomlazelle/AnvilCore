namespace AnvilCore;

public interface IAnvilData
{
    object Create(Type type);
    T Create<T>();
    IEnumerable<T> CreateMany<T>(int count = 3);
}
