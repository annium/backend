using System;
using System.Reflection;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Models;
using Annium.linq2db.Testing.Sqlite.Internal;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;

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
        container.Add(_ =>
        {
            var testingReference = new TestingSqliteReference();

            Migrator.Execute(migrationsAssembly, testingReference.ConnectionString);

            return testingReference;
        }).AsSelf().Singleton();

        container.Add(sp =>
        {
            var builder = new LinqToDBConnectionOptionsBuilder();

            var connectionString = sp.Resolve<TestingSqliteReference>().ConnectionString;
            builder.UseConnectionString(ProviderName.SQLiteMS, connectionString);

            var mappingSchema = new MappingSchema();
            mappingSchema
                .ApplyConfigurations(sp)
                .UseSnakeCaseColumns()
                .UseJsonSupport(sp);
            configure(sp, mappingSchema);
            builder.UseMappingSchema(mappingSchema);
            builder.UseLogging<TConnection>(sp);

            var options = builder.Build();

            return new ConfigurationContainer<TConnection>(options);
        }).AsSelf().Singleton();

        container.Add(sp =>
        {
            var configurationContainer = sp.Resolve<ConfigurationContainer<TConnection>>();

            return (TConnection) Activator.CreateInstance(typeof(TConnection), configurationContainer.Options)!;
        }).AsSelf().Transient();

        container.AddEntityConfigurations();

        return container;
    }

    /// <summary>
    /// Configuration container per <see cref="TConnection"/>
    /// </summary>
    /// <param name="Schema"></param>
    /// <typeparam name="TConnection">Connection type, configuration is specific for</typeparam>
    private sealed record ConfigurationContainer<TConnection>(LinqToDBConnectionOptions Options);
}