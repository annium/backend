using System.Collections.Generic;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class Configuration : IConfiguration
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

    internal class InMemoryConfiguration : IConfiguration
    {
        public ISerializer<string> Serializer { get; }
        public IList<string>? MessageBox { get; }

        public InMemoryConfiguration(
            ISerializer<string> serializer,
            IList<string>? messageBox
        )
        {
            Serializer = serializer;
            MessageBox = messageBox;
        }
    }

    internal interface IConfiguration
    {
        ISerializer<string> Serializer { get; }
    }
}