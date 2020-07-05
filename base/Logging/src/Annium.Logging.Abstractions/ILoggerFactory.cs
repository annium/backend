namespace Annium.Logging.Abstractions
{
    public interface ILoggerFactory
    {
        ILogger<T> GetLogger<T>();
    }
}