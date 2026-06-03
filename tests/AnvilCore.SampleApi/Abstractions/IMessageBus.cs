namespace AnvilCore.SampleApi;

public interface IMessageBus
{
    void Publish(object message);
}
