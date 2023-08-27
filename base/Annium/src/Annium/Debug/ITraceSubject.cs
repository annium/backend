namespace Annium.Debug;

public interface ITraceSubject<out T>
{
    ITracer Tracer { get; }
}