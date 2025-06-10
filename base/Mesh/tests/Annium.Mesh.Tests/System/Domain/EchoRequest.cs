using MessagePack;

namespace Annium.Mesh.Tests.System.Domain;

/// <summary>
/// Represents an echo request message containing a string message, used for testing request-response patterns in mesh communication.
/// </summary>
[MessagePackObject]
public class EchoRequest
{
    /// <summary>
    /// Gets the message to be echoed back by the server.
    /// </summary>
    [Key(0)]
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoRequest"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message to be echoed back by the server.</param>
    public EchoRequest(string message)
    {
        Message = message;
    }
}
