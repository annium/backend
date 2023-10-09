namespace Annium.Mesh.Transport.Abstractions;

public interface IClientConnectionFactory
{
    IClientConnection Create();
}