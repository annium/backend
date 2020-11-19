using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Obsolete
{
    [Obsolete]
    public interface IServiceProviderBuilder
    {
        IServiceProviderBuilder UseServicePack<TServicePack>()
            where TServicePack : ServicePackBase, new();

        ServiceProvider Build();
    }
}