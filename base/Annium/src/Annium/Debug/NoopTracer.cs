namespace Annium.Debug;

public class NoopTracer : ITracer
{
    // TODO: perhaps, drop, cause all injection will be through DI
    public static readonly ITracer Instance = new NoopTracer();

    private NoopTracer()
    {
    }

    public void Trace<T>(
        T subject,
        string message,
        bool withTrace,
        string file,
        string member,
        int line
    )
        where T : notnull
    {
    }
}