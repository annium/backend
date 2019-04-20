using Annium.Testing.Logging;

namespace Annium.Testing
{
    public class Configuration
    {
        public LoggerConfiguration Logger { get; }

        public string Filter { get; }

        public Configuration(
            LoggerConfiguration logger,
            string filter
        )
        {
            Logger = logger;
            Filter = filter;
        }
    }
}