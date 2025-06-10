using System.Text.Json.Serialization;
using Annium.Core.Runtime.Types;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

/// <summary>
/// Represents a generic message with typed content that can be transmitted through the MessageBus.
/// </summary>
/// <typeparam name="T">The type of the message content.</typeparam>
internal record Message<T> : Message, IMessage<T>
{
    /// <summary>
    /// Gets the content of the message.
    /// </summary>
    [JsonPropertyName("v")]
    public T Content { get; }

    /// <summary>
    /// Initializes a new instance of the Message class with the specified content.
    /// </summary>
    /// <param name="content">The content of the message.</param>
    public Message(T content)
    {
        Content = content;
    }
}

/// <summary>
/// Defines a contract for messages that contain typed content.
/// </summary>
/// <typeparam name="T">The type of the message content.</typeparam>
internal interface IMessage<out T>
{
    /// <summary>
    /// Gets the content of the message.
    /// </summary>
    T Content { get; }
}

/// <summary>
/// Base class for all messages, providing type identification for serialization.
/// </summary>
internal abstract record Message
{
    /// <summary>
    /// Gets the type identifier string for this message type.
    /// </summary>
    [ResolutionId]
    [JsonPropertyName("t")]
    public string Tid => GetType().GetIdString();
}
