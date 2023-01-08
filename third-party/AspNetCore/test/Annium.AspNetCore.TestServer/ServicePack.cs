using System;
using Annium.Core.DependencyInjection;

namespace Annium.AspNetCore.TestServer;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Console.WriteLine(nameof(ServicePack));
        Add<BaseServicePack>();
    }
}