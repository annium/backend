using System;
using System.Reflection;
using Annium.linq2db.Extensions;
using Annium.linq2db.Testing.Sqlite;
using FluentMigrator.Runner;
using LinqToDB;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddTestingSqlite<TConnection>(
        this IServiceContainer container,
        Assembly migrationsAssembly
    )
        where TConnection : DataConnectionBase
    {
        return container
            .AddTestingSqlite<TConnection>(
                Assembly.GetCallingAssembly(),
                migrationsAssembly,
                _ => { }
            );
    }

    public static IServiceContainer AddTestingSqlite<TConnection>(
        this IServiceContainer container,
        Assembly migrationsAssembly,
        Action<MappingSchema> configure
    )
        where TConnection : DataConnectionBase
    {
        return container
            .AddTestingSqlite<TConnection>(
                Assembly.GetCallingAssembly(),
                migrationsAssembly,
                configure
            );
    }

    public static IServiceContainer AddTestingSqlite<TConnection>(
        this IServiceContainer container,
        Assembly configurationsAssembly,
        Assembly migrationsAssembly
    )
        where TConnection : DataConnectionBase
    {
        return container
            .AddTestingSqlite<TConnection>(
                configurationsAssembly,
                migrationsAssembly,
                _ => { }
            );
    }

    public static IServiceContainer AddTestingSqlite<TConnection>(
        this IServiceContainer container,
        Assembly configurationsAssembly,
        Assembly migrationsAssembly,
        Action<MappingSchema> configure
    )
        where TConnection : DataConnectionBase
    {
        var mappingSchema = new MappingSchema();
        mappingSchema.GetMappingBuilder(configurationsAssembly)
            .ApplyConfigurations()
            .SnakeCaseColumns();
        configure(mappingSchema);

        container.Add(_ =>
        {
            var testingReference = new TestingSqliteReference();

            var migrationRunner = CreateMigrationRunner(testingReference.ConnectionString, migrationsAssembly);
            migrationRunner.MigrateUp();

            return testingReference;
        }).Singleton();


        container.Add(sp => (TConnection) Activator.CreateInstance(
            typeof(TConnection),
            ProviderName.SQLiteMS,
            sp.Resolve<TestingSqliteReference>().ConnectionString,
            mappingSchema
        )!).Scoped();

        return container;
    }

    private static IMigrationRunner CreateMigrationRunner(string connectionString, Assembly migrationsAssembly)
    {
        var container = new ServiceContainer();
        container.Collection
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(migrationsAssembly).For.Migrations()
            );

        return container.BuildServiceProvider().Resolve<IMigrationRunner>();
    }
}