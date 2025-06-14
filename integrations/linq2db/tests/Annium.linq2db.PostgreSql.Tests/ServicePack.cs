using System;
using Annium.Core.DependencyInjection;
using Annium.linq2db.PostgreSql.Tests.Db;
using Annium.linq2db.Tests.Lib.Db;

namespace Annium.linq2db.PostgreSql.Tests;

/// <summary>
/// Service pack for configuring PostgreSQL linq2db test dependencies and database setup
/// </summary>
internal class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers PostgreSQL linq2db services and test database configuration
    /// </summary>
    /// <param name="container">Service container for dependency registration</param>
    /// <param name="provider">Service provider for dependency resolution</param>
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddPostgreSql<Connection>();
        container.Add<Database>().AsSelf().Singleton();
        container.Add(sp => sp.Resolve<Database>().Config).AsSelf().Singleton();
    }

    /// <summary>
    /// Sets up the test environment by initializing the PostgreSQL database container
    /// </summary>
    /// <param name="provider">Service provider for dependency resolution</param>
    public override void Setup(IServiceProvider provider)
    {
#pragma warning disable VSTHRD002
        provider.Resolve<Database>().InitAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }
}
