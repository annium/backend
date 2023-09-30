namespace Annium.Mesh.Client;

public interface IClientFactory
{
    IClient Create(IClientConfiguration configuration);
}