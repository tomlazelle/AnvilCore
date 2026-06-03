namespace AnvilCore;

public interface IAnvilHostStrategy
{
    ValueTask<IAnvilTestApplication> BuildAsync(IAnvilTestApplicationBuilder builder, CancellationToken cancellationToken);
}
