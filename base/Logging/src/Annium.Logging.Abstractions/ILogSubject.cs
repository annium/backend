namespace Annium.Logging.Abstractions;

public interface ILogSubject<out T>
{
    ILogger<T> Logger { get; }
}

public interface ILogSubject
{
    ILogger Logger { get; }
}