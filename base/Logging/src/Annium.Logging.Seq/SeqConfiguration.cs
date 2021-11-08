using System;
using Annium.Logging.Shared;

namespace Annium.Logging.Seq
{
    public record SeqConfiguration : LogRouteConfiguration
    {
        public Uri Endpoint { get; }
        public string ApiKey { get; }
        public string Project { get; }

        public SeqConfiguration(
            Uri endpoint,
            string apiKey,
            string project
        ) : base(TimeSpan.FromSeconds(5), 100)
        {
            Endpoint = endpoint;
            ApiKey = apiKey;
            Project = project;
        }

        public SeqConfiguration(
            Uri endpoint,
            string apiKey,
            string project,
            TimeSpan bufferTime,
            int bufferCount
        ) : base(bufferTime, bufferCount)
        {
            Endpoint = endpoint;
            ApiKey = apiKey;
            Project = project;
        }
    }
}