using System;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Transport
{
    public interface IInMemoryConfigurationBuilder
    {
        IInMemoryConfigurationBuilder WithMessageBox(Action<string> addMessage);
        IInMemoryConfigurationBuilder WithSerializer(ISerializer<string> serializer);
    }
}