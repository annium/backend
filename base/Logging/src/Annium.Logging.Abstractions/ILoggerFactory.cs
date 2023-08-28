namespace Annium.Logging.Abstractions;

public interface ILoggerFactory
{
    ILogger Get<T>();
}