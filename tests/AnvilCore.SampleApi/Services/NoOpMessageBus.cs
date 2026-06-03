namespace AnvilCore.SampleApi;

public sealed class NoOpMessageBus : IMessageBus
{
    public void Publish(object message) { }
}
