namespace Annium.Mesh.Server.Models;

public interface IRequestContext<TRequest>
{
    TRequest Request { get; }
}
