using Annium.Mesh.Domain;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Mesh.Serialization.MessagePack.Internal;

/// <summary>
/// Custom formatter resolver that provides MessagePack formatters for mesh domain types.
/// </summary>
internal class FormatterResolver : IFormatterResolver
{
    /// <summary>
    /// Gets the appropriate formatter for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to get a formatter for.</typeparam>
    /// <returns>A formatter for the specified type, or null if not supported.</returns>
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        if (typeof(T) == typeof(Message))
            return (IMessagePackFormatter<T>)new MessageFormatter();

        return null;
    }
}
