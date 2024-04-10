using System;
using Annium.Core.DependencyInjection;

namespace Annium.linq2db.Tests.Lib;

public class LibServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithManagedTime().SetDefault();
        container.AddSerializers().WithJson(opts => opts.UseCamelCaseNamingPolicy());
    }
}
