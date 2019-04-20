using System.Xml.Linq;
using Annium.Testing.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Annium.Testing.TestAdapter
{
    internal static class ConfigurationReader
    {
        public static Configuration Read(IDiscoveryContext context)
        {
            var logLevel = GetLogLevel(XElement.Parse(context.RunSettings.SettingsXml).Element("logLevel"));
            var filter = XElement.Parse(context.RunSettings.SettingsXml).Element("filter")?.Value;

            var loggerConfiguration = new LoggerConfiguration(logLevel);
            var configuration = new Configuration(loggerConfiguration, filter);

            return configuration;
        }

        private static LogLevel GetLogLevel(XElement node)
        {
            switch (node?.Value)
            {
                case "debug":
                    return LogLevel.Debug;
                case "trace":
                    return LogLevel.Trace;
                default:
                    return LogLevel.Info;
            }
        }
    }
}