namespace Annium.Debug;

public interface ITracer
{
    void Trace<T>(
        T subject,
        string message,
        bool withTrace,
        string file,
        string member,
        int line
    )
        where T : notnull;
}