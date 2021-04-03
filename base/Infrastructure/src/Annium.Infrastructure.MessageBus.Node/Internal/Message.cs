using System.Text.Json.Serialization;
using Annium.Core.Runtime.Types;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal record Message<T> : Message, IMessage<T>
    {
        [JsonPropertyName("v")]
        public T Content { get; }

        public Message(T content)
        {
            Content = content;
        }
    }

    internal interface IMessage<out T>
    {
        T Content { get; }
    }

    internal abstract record Message
    {
        [ResolutionId]
        [JsonPropertyName("t")]
        public string Tid => GetType().GetIdString();
    }
}