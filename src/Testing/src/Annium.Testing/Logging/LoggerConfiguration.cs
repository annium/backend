namespace Annium.Testing.Logging
{
    public class LoggerConfiguration
    {
        public LogLevel LogLevel { get; }

        public LoggerConfiguration(
            LogLevel logLevel
        )
        {
            LogLevel = logLevel;
        }
    }
}