namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Well-known keys for the message-bus registration surface.
/// </summary>
public static class MessageBusKeys
{
    /// <summary>
    /// The reserved key under which a keyless <c>Add…MessageBus</c> registration is placed. The same registration is
    /// additionally exposed non-keyed (so <c>Resolve&lt;IMessagePublisher&gt;()</c> works), which makes it the default
    /// broker. A keyed registration must not reuse this key.
    /// </summary>
    public const string Default = "default";
}
