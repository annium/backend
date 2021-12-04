using System.Xml.Linq;
using Annium.Logging.Abstractions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Annium.Testing.TestAdapter;

internal static class TestingConfigurationReader
{
    public static TestingConfiguration Read(IDiscoveryContext context)
    {
        var logLevel = GetLogLevel(XElement.Parse(context.RunSettings.SettingsXml).Element("logLevel"!)!);
        var filter = XElement.Parse(context.RunSettings.SettingsXml).Element("filter"!)?.Value ?? string.Empty;

        var configuration = new TestingConfiguration(logLevel, filter);

        return configuration;
    }

    private static LogLevel GetLogLevel(XElement node) => node.Value switch
    {
        "debug" => LogLevel.Debug,
        "trace" => LogLevel.Trace,
        _       => LogLevel.Info,
    };
}