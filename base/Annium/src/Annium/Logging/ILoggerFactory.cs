namespace Annium.Logging;

public interface ILoggerFactory
{
    ILogger Get<T>();
}