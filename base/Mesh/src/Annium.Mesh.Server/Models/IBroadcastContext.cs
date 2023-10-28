namespace Annium.Mesh.Server.Models;

public interface IBroadcastContext<TMessage>
{
    public void Send(TMessage message);
}
