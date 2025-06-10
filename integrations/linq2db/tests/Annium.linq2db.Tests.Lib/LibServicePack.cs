using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Packs;
using Annium.Core.Runtime;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;

namespace Annium.linq2db.Tests.Lib;

public class LibServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithManagedTime().SetDefault();
        container.AddSerializers().WithJson(opts => opts.UseCamelCaseNamingPolicy());
    }
}
