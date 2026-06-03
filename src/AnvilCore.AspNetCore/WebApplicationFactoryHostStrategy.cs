namespace AnvilCore.AspNetCore;

internal sealed class WebApplicationFactoryHostStrategy<TEntryPoint> : IAnvilHostStrategy
    where TEntryPoint : class
{
    public ValueTask<IAnvilTestApplication> BuildAsync(
        IAnvilTestApplicationBuilder builder,
        CancellationToken cancellationToken)
    {
        var factory = new AnvilWebApplicationFactory<TEntryPoint>(builder);
        return ValueTask.FromResult<IAnvilTestApplication>(new AnvilWebApplication<TEntryPoint>(factory));
    }
}
