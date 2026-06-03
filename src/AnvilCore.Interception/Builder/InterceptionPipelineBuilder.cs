namespace AnvilCore.Interception;

public sealed class InterceptionPipelineBuilder
{
    private readonly List<IAnvilInterceptor> _interceptors = [];

    public InterceptionPipelineBuilder Capture()
    {
        _interceptors.Add(new CaptureInterceptor());
        return this;
    }

    public InterceptionPipelineBuilder Suppress()
    {
        _interceptors.Add(new SuppressInterceptor());
        return this;
    }

    public InterceptionPipelineBuilder Continue()
    {
        _interceptors.Add(new ContinueInterceptor());
        return this;
    }

    public InterceptionPipelineBuilder Return(object? value)
    {
        _interceptors.Add(new ReturnValueInterceptor(value));
        return this;
    }

    public InterceptionPipelineBuilder ReturnFixture()
    {
        _interceptors.Add(new ReturnFixtureInterceptor());
        return this;
    }

    public InterceptionPipelineBuilder Throw(Exception exception)
    {
        _interceptors.Add(new ThrowInterceptor(exception));
        return this;
    }

    public InterceptionPipelineBuilder Delay(TimeSpan duration)
    {
        _interceptors.Add(new DelayInterceptor(duration));
        return this;
    }

    public InterceptionPipelineBuilder Use(
        Func<AnvilInvocationContext, AnvilNextDelegate, CancellationToken, ValueTask<object?>> handler)
    {
        _interceptors.Add(new DelegateInterceptor(handler));
        return this;
    }

    internal AnvilInterceptorPipeline Build() => new(_interceptors.AsReadOnly());
}
