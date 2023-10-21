using Annium.Mesh.Domain;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Mesh.Serialization.MessagePack.Internal;

internal class FormatterResolver : IFormatterResolver
{
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        if (typeof(T) == typeof(Message))
            return (IMessagePackFormatter<T>)new MessageFormatter();

        return null;
    }
}