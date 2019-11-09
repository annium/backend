using System.Text.Json;
using Annium.Core.DependencyInjection;
using NodaTime;

namespace Annium.Net.WebSockets.Internal
{
    internal static class Options
    {
        public static readonly JsonSerializerOptions Json = new JsonSerializerOptions()
            .ConfigureAbstractConverter()
            .ConfigureForNodaTime(DateTimeZoneProviders.Serialization);
    }
}