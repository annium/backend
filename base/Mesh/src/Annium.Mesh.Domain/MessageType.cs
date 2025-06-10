namespace Annium.Mesh.Domain;

/// <summary>
/// Defines the types of messages that can be sent through the mesh communication system.
/// </summary>
public enum MessageType : byte
{
    /// <summary>
    /// No message type specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that a connection is ready for communication.
    /// </summary>
    ConnectionReady = 1,

    /// <summary>
    /// A request message that expects a response.
    /// </summary>
    Request = 10,

    /// <summary>
    /// A response message to a previous request.
    /// </summary>
    Response = 11,

    /// <summary>
    /// An event message that notifies about something that happened.
    /// </summary>
    Event = 20,

    /// <summary>
    /// A push message sent to specific clients without requiring a response.
    /// </summary>
    Push = 21,

    /// <summary>
    /// Initializes a subscription for receiving ongoing messages.
    /// </summary>
    SubscriptionInit = 30,

    /// <summary>
    /// Confirms that a subscription has been established.
    /// </summary>
    SubscriptionConfirm = 31,

    /// <summary>
    /// Cancels an existing subscription.
    /// </summary>
    SubscriptionCancel = 32,

    /// <summary>
    /// A message sent as part of an active subscription.
    /// </summary>
    SubscriptionMessage = 33,
}
