namespace Annium.Mesh.Server.Models;

public interface IPushContext<TMessage>
{
    void Send(TMessage message);
}
