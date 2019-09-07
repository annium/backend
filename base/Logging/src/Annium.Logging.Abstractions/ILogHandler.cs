namespace Annium.Logging.Abstractions
{
    public interface ILogHandler
    {
        void Handle(LogMessage message);
    }
}