namespace Annium.Infrastructure.WebSockets.Client
{
    public interface IClientFactory
    {
        IClientBase Create(ClientConfiguration configuration);
    }
}