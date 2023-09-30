using Annium.Core.DependencyInjection;

namespace Demo.Mesh.Domain;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Annium.Mesh.Domain.ServicePack>();
    }
}