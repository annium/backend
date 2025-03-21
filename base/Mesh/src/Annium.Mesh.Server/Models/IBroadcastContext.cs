namespace Annium.Mesh.Server.Models;

public interface IBroadcastContext<TMessage>
    where TMessage : notnull
{
    void Send(TMessage message);
}
