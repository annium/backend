using System;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.linq2db.Tests.Lib.Db;

namespace Annium.linq2db.Testing.Sqlite.Tests;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTestingSqlite<Connection>(Assembly.GetExecutingAssembly());
    }
}