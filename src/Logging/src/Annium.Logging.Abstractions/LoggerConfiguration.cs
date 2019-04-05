namespace Annium.Logging.Abstractions
{
    public class LoggerConfiguration
    {
        public LogLevel LogLevel { get; }

        public LoggerConfiguration(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }
    }
}