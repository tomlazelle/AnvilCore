namespace AnvilCore.AspNetCore;

public static class AnvilBuilderAspNetCoreExtensions
{
    public static AnvilTestApplicationBuilder UseWebApplicationFactory<TEntryPoint>(
        this AnvilTestApplicationBuilder builder)
        where TEntryPoint : class
    {
        return builder.UseHostStrategy(new WebApplicationFactoryHostStrategy<TEntryPoint>());
    }
}
