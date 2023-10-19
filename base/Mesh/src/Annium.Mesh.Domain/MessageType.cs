namespace Annium.Mesh.Domain;

public enum MessageType
{
    None = 0,
    ConnectionReady = 1,
    Request = 10,
    Response = 11,
    Event = 20,
    Push = 21,
    SubscriptionInit = 30,
    SubscriptionConfirm = 31,
    SubscriptionCancel = 32,
    SubscriptionMessage = 33,
}