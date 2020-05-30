using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using NodaTime.Xml;

namespace Annium.Net.WebSockets
{
    public static class Serializers
    {
        public static readonly ISerializer<byte[]> Json = ByteArraySerializer.Configure(
            opts => opts.ConfigureDefault()
                .ConfigureForOperations()
                .ConfigureForNodaTime(XmlSerializationSettings.DateTimeZoneProvider)
        );
    }
}