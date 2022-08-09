using System;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.linq2db.Extensions.Tests.Db;

namespace Annium.linq2db.Extensions.Tests;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(Assembly.GetExecutingAssembly());
        container.AddTestingSqlite<Connection>(Assembly.GetExecutingAssembly());
        container.AddJsonSerializers().SetDefault();
    }
}