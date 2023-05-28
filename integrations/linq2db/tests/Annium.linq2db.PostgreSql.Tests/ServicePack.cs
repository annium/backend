using System;
using Annium.Core.DependencyInjection;
using Annium.linq2db.PostgreSql.Tests.Db;
using Annium.linq2db.Tests.Lib.Db;

namespace Annium.linq2db.PostgreSql.Tests;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddPostgreSql<Connection>();
        container.Add(Database.Config).AsSelf().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        Database.AcquireAsync().GetAwaiter().GetResult();
    }
}