using System;
using Annium.linq2db.Extensions;
using Annium.linq2db.PostgreSql;
using Annium.Logging;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Mapping;
using Npgsql;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TConnection : DataConnection, ILogSubject
    {
        return container.AddPostgreSql<TConnection>(
            sp => sp.Resolve<PostgreSqlConfiguration>(),
            (_, _) => { },
            lifetime
        );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        Action<IServiceProvider, MappingSchema> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TConnection : DataConnection, ILogSubject
    {
        return container
            .AddPostgreSql<TConnection>(
                sp => sp.Resolve<PostgreSqlConfiguration>(),
                configure,
                lifetime
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        PostgreSqlConfiguration cfg,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TConnection : DataConnection, ILogSubject
    {
        return container
            .AddPostgreSql<TConnection>(
                _ => cfg,
                (_, _) => { },
                lifetime
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        PostgreSqlConfiguration cfg,
        Action<IServiceProvider, MappingSchema> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TConnection : DataConnection, ILogSubject
    {
        return container
            .AddPostgreSql<TConnection>(
                _ => cfg,
                configure,
                lifetime
            );
    }

    private static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        Func<IServiceProvider, PostgreSqlConfiguration> getCfg,
        Action<IServiceProvider, MappingSchema> configure,
        ServiceLifetime lifetime
    )
        where TConnection : DataConnection, ILogSubject
    {
        // mapping schema
        container.Add(sp =>
        {
            var mappingSchema = new MappingSchema();
            mappingSchema
                .ApplyConfigurations(sp)
                .UseSnakeCaseColumns()
                .UseJsonSupport(sp);
            configure(sp, mappingSchema);

            return new MappingSchemaContainer<TConnection>(mappingSchema);
        }).AsSelf().Singleton();

        // data source
        container.Add(sp =>
        {
            var cfg = getCfg(sp);
            var logger = sp.Resolve<ILogger>();

            // configure data source and NodaTime
            var connectionString = cfg.ConnectionString;
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseNodaTime();
            var dataSource = dataSourceBuilder.Build();

            return new DataSourceContainer<TConnection>(dataSource, logger);
        }).AsSelf().Singleton();

        container.Add(sp =>
        {
            var mappingSchema = sp.Resolve<MappingSchemaContainer<TConnection>>().Schema;
            var dataSource = sp.Resolve<DataSourceContainer<TConnection>>().DataSource;
            var connection = dataSource.CreateConnection();

            var options = new DataOptions()
                .UseConnection(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v15), connection, true)
                .UseMappingSchema(mappingSchema)
                .UseLogging(sp);

            return new DataOptions<TConnection>(options);
        }).AsSelf().In(lifetime);

        container.Add<TConnection>().AsSelf().In(lifetime);

        container.AddEntityConfigurations();

        return container;
    }
}

// ReSharper disable once UnusedTypeParameter
file sealed record MappingSchemaContainer<TConnection>(MappingSchema Schema);

// ReSharper disable once UnusedTypeParameter
file sealed record DataSourceContainer<TConnection> : IDisposable, ILogSubject
{
    public NpgsqlDataSource DataSource { get; }
    public ILogger Logger { get; }

    public DataSourceContainer(NpgsqlDataSource dataSource, ILogger logger)
    {
        DataSource = dataSource;
        Logger = logger;
        this.Trace("created");
    }

    public void Dispose()
    {
        DataSource.Dispose();
        this.Trace("disposed");
    }
}