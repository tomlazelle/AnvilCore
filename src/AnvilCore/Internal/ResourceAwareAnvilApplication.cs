using System.Net.Http;

namespace AnvilCore;

internal sealed class ResourceAwareAnvilApplication : IAnvilTestApplication
{
    private readonly IAnvilTestApplication _inner;
    private readonly IReadOnlyList<Func<ValueTask>> _disposeCallbacks;

    internal ResourceAwareAnvilApplication(
        IAnvilTestApplication inner,
        IReadOnlyList<Func<ValueTask>> disposeCallbacks)
    {
        _inner = inner;
        _disposeCallbacks = disposeCallbacks;
    }

    public IServiceProvider Services => _inner.Services;
    public IAnvilData Data => _inner.Data;
    public IAnvilCaptureStore Captured => _inner.Captured;
    public HttpClient CreateClient() => _inner.CreateClient();

    public async ValueTask DisposeAsync()
    {
        var exceptions = new List<Exception>();

        try
        {
            await _inner.DisposeAsync();
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        foreach (var callback in _disposeCallbacks)
        {
            try
            {
                await callback();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count == 1)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
        else if (exceptions.Count > 1)
            throw new AggregateException(exceptions);
    }
}
