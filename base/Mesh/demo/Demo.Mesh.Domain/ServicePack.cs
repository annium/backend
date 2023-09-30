using Annium.Core.DependencyInjection;

namespace Demo.Infrastructure.WebSockets.Domain;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Annium.Infrastructure.WebSockets.Domain.ServicePack>();
    }
}