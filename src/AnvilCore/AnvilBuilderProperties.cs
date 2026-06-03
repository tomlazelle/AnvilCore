namespace AnvilCore;

public static class AnvilBuilderProperties
{
    public const string PostConfigureActions = "anvil:post-configure-actions";
    public const string PreBuildAsyncActions = "anvil:pre-build-async-actions";
    public const string AppDisposeCallbacks  = "anvil:app-dispose-callbacks";
    public const string ResourceConfig       = "anvil:resource-config";
    public const string Logger               = "anvil:logger";

    public static void Log(IDictionary<string, object> properties, string message)
    {
        if (properties.TryGetValue(Logger, out var obj) && obj is Action<string> logger)
            logger(message);
    }
}
