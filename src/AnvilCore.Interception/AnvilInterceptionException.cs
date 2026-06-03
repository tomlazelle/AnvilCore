namespace AnvilCore.Interception;

public sealed class AnvilInterceptionException : Exception
{
    public AnvilInterceptionException(string message) : base(message) { }
    public AnvilInterceptionException(string message, Exception inner) : base(message, inner) { }
}
