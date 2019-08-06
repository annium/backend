using Annium.Testing.Logging;

namespace Annium.Testing
{
    public class TestingConfiguration
    {
        public LoggerConfiguration Logger { get; }

        public string Filter { get; }

        public TestingConfiguration(
            LoggerConfiguration logger,
            string filter
        )
        {
            Logger = logger;
            Filter = filter;
        }
    }
}