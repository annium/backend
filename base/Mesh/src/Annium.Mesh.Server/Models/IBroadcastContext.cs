namespace Annium.Mesh.Server.Models;

public interface IBroadcastContext<TMessage>
    where TMessage : notnull
{
    public void Send(TMessage message);
}
