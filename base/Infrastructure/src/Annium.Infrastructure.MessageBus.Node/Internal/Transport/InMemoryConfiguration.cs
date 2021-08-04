using System;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport
{
    internal class InMemoryConfiguration : IConfiguration
    {
        public ISerializer<string> Serializer { get; }
        public Action<string>? AddMessage { get; }

        public InMemoryConfiguration(
            ISerializer<string> serializer,
            Action<string>? addMessage
        )
        {
            Serializer = serializer;
            AddMessage = addMessage;
        }
    }
}