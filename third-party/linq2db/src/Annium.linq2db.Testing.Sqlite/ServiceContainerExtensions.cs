using System;
using System.Reflection;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Models;
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
                (_, _) => { }
            );
    }

    public static IServiceContainer AddTestingSqlite<TConnection>(
        this IServiceContainer container,
        Assembly migrationsAssembly,
        Action<IServiceProvider, MappingSchema> configure
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
                (_, _) => { }
            );
    }

    public static IServiceContainer AddTestingSqlite<TConnection>(
        this IServiceContainer container,
        Assembly configurationsAssembly,
        Assembly migrationsAssembly,
        Action<IServiceProvider, MappingSchema> configure
    )
        where TConnection : DataConnectionBase
    {
        container.Add(_ =>
        {
            var testingReference = new TestingSqliteReference();

            var migrationRunner = CreateMigrationRunner(testingReference.ConnectionString, migrationsAssembly);
            migrationRunner.MigrateUp();

            return testingReference;
        }).AsSelf().Singleton();

        container.Add(sp =>
        {
            var mappingSchema = new MappingSchema();
            mappingSchema
                .ApplyConfigurations(sp)
                .UseSnakeCaseColumns()
                .UseJsonSupport(sp);
            configure(sp, mappingSchema);

            return mappingSchema;
        }).AsSelf().Singleton();

        container.Add(sp =>
        {
            var connectionString = sp.Resolve<TestingSqliteReference>().ConnectionString;
            var mappingSchema = sp.Resolve<ConfigurationContainer>().Schema;

            return (TConnection) Activator.CreateInstance(
                typeof(TConnection),
                ProviderName.SQLiteMS,
                connectionString,
                mappingSchema
            )!;
        }).AsSelf().Transient();

        container.AddEntityConfigurations();
        container.AddRepositories();

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

    private sealed record ConfigurationContainer(MappingSchema Schema);
}