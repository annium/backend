using System.Collections.Generic;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport
{
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
}