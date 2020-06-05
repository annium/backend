using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using NodaTime.Xml;

namespace Annium.Net.Http.Internal
{
    internal static class Serializers
    {
        public static readonly ISerializer<string> Json = StringSerializer.Configure(
            opts => opts.ConfigureDefault()
                .ConfigureForOperations()
                .ConfigureForNodaTime(XmlSerializationSettings.DateTimeZoneProvider)
        );
    }
}