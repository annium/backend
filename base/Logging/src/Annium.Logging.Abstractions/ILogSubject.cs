namespace Annium.Logging.Abstractions
{
    public interface ILogSubject
    {
        ILogger Logger { get; }
    }

    public interface ILogSubject<out T> : ILogSubject
    {
    }
}