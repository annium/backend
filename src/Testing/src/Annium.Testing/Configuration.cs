using Annium.Testing.Logging;

namespace Annium.Testing
{
    public class Configuration
    {
        public LoggerConfiguration Logger { get; }

        public Configuration(
            LoggerConfiguration logger
        )
        {
            Logger = logger;
        }
    }
}