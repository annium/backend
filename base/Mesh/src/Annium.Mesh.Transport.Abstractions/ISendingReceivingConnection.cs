namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Represents a connection that supports both sending and receiving data
/// </summary>
public interface ISendingReceivingConnection : ISendingConnection, IReceivingConnection;
