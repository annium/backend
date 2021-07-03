using System;
using Annium.Logging.Shared;

namespace Annium.Logging.Seq
{
    public class SeqConfiguration : LogRouteConfiguration
    {
        public Uri Endpoint { get; }
        public string ApiKey { get; }

        public SeqConfiguration(
            Uri endpoint,
            string apiKey
        ) : base(TimeSpan.FromSeconds(5), 100)
        {
            Endpoint = endpoint;
            ApiKey = apiKey;
        }

        public SeqConfiguration(
            Uri endpoint,
            string apiKey,
            TimeSpan bufferTime,
            int bufferCount
        ) : base(bufferTime, bufferCount)
        {
            Endpoint = endpoint;
            ApiKey = apiKey;
        }
    }
}