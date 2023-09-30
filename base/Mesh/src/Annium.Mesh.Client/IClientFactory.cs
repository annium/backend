namespace Annium.Infrastructure.WebSockets.Client;

public interface IClientFactory
{
    IClient Create(IClientConfiguration configuration);
}