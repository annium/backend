namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal interface IRequestContextInternal
    {
        ConnectionState StateInternal { get; }
    }
}