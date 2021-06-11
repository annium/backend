namespace Annium.Logging.Shared
{
    public interface ILogHandler
    {
        void Handle(LogMessage message);
    }
}