using Xunit.Abstractions;

namespace AnvilCore.Xunit;

public static class AnvilBuilderXunitExtensions
{
    public static IAnvilTestApplicationBuilder UseXUnit(
        this IAnvilTestApplicationBuilder builder,
        ITestOutputHelper output)
    {
        builder.Properties[AnvilBuilderProperties.Logger] =
            (Action<string>)(msg => output.WriteLine(msg));
        return builder;
    }
}
