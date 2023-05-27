using System;
using System.Reflection;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Testing.Sqlite.Internal;
using Annium.Logging.Abstractions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddTestingSqlite<TConnection>(
        this IServiceContainer container,
        Assembly migrationsAssembly
    )
        where TConnection : DataConnection, ILogSubject<TConnection>
    {
        return container
            .AddTestingSqlite<TConnection>(
                migrationsAssembly,
                (_, _) => { }
            );
    }

    public static IServiceContainer AddTestingSqlite<TConnection>(
        this IServiceContainer container,
        Assembly migrationsAssembly,
        Action<IServiceProvider, MappingSchema> configure
    )
        where TConnection : DataConnection, ILogSubject<TConnection>
    {
        container.Add(_ =>
        {
            var testingReference = new TestingSqliteReference();

            Migrator.Execute(migrationsAssembly, testingReference.ConnectionString);

            return testingReference;
        }).AsSelf().Singleton();

        container.Add(sp =>
        {
            var connectionString = sp.Resolve<TestingSqliteReference>().ConnectionString;

            var mappingSchema = new MappingSchema();
            mappingSchema
                .ApplyConfigurations(sp)
                .UseSnakeCaseColumns()
                .UseJsonSupport(sp);
            configure(sp, mappingSchema);

            var options = new DataOptions()
                .UseConnectionString(ProviderName.SQLiteMS, connectionString)
                .UseMappingSchema(mappingSchema)
                .UseLogging<TConnection>(sp);

            return new DataOptions<TConnection>(options);
        }).AsSelf().Singleton();

        container.Add<TConnection>().AsSelf().Transient();

        container.AddEntityConfigurations();

        return container;
    }
}