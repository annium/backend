using System.Collections.Generic;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class Configuration
    {
        public EndpointsConfiguration Endpoints { get; }
        public ISerializer<string> Serializer { get; }

        public Configuration(
            EndpointsConfiguration endpoints,
            ISerializer<string> serializer
        )
        {
            Endpoints = endpoints;
            Serializer = serializer;
        }
    }

    internal class TestConfiguration
    {
        public ISerializer<string> Serializer { get; }
        public IList<string> MessageBox { get; }

        public TestConfiguration(
            ISerializer<string> serializer,
            IList<string> messageBox
        )
        {
            Serializer = serializer;
            MessageBox = messageBox;
        }
    }
}