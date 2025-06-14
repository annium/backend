using System;
using Annium.Core.DependencyInjection;
using Annium.linq2db.Extensions;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.Logging;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Mapping;
using Npgsql;

namespace Annium.linq2db.PostgreSql;

/// <summary>
/// Extension methods for configuring PostgreSQL with linq2db in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds PostgreSQL services using configuration from the service provider
    /// </summary>
    /// <typeparam name="TConnection">The DataConnection type</typeparam>
    /// <param name="container">The service container to configure</param>
    /// <param name="lifetime">The service lifetime for the connection</param>
    /// <returns>The configured service container for chaining</returns>
    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TConnection : DataConnection
    {
        return container.AddPostgreSql<TConnection>(
            sp => sp.Resolve<PostgreSqlConfiguration>(),
            (_, _) => { },
            lifetime
        );
    }

    /// <summary>
    /// Adds PostgreSQL services with custom mapping schema configuration
    /// </summary>
    /// <typeparam name="TConnection">The DataConnection type</typeparam>
    /// <param name="container">The service container to configure</param>
    /// <param name="configure">Action to configure the mapping schema</param>
    /// <param name="lifetime">The service lifetime for the connection</param>
    /// <returns>The configured service container for chaining</returns>
    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        Action<IServiceProvider, MappingSchema> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TConnection : DataConnection
    {
        return container.AddPostgreSql<TConnection>(sp => sp.Resolve<PostgreSqlConfiguration>(), configure, lifetime);
    }

    /// <summary>
    /// Adds PostgreSQL services with a specific configuration
    /// </summary>
    /// <typeparam name="TConnection">The DataConnection type</typeparam>
    /// <param name="container">The service container to configure</param>
    /// <param name="cfg">The PostgreSQL configuration</param>
    /// <param name="lifetime">The service lifetime for the connection</param>
    /// <returns>The configured service container for chaining</returns>
    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        PostgreSqlConfiguration cfg,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TConnection : DataConnection
    {
        return container.AddPostgreSql<TConnection>(_ => cfg, (_, _) => { }, lifetime);
    }

    /// <summary>
    /// Adds PostgreSQL services with a specific configuration and custom mapping schema configuration
    /// </summary>
    /// <typeparam name="TConnection">The DataConnection type</typeparam>
    /// <param name="container">The service container to configure</param>
    /// <param name="cfg">The PostgreSQL configuration</param>
    /// <param name="configure">Action to configure the mapping schema</param>
    /// <param name="lifetime">The service lifetime for the connection</param>
    /// <returns>The configured service container for chaining</returns>
    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        PostgreSqlConfiguration cfg,
        Action<IServiceProvider, MappingSchema> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TConnection : DataConnection
    {
        return container.AddPostgreSql<TConnection>(_ => cfg, configure, lifetime);
    }

    /// <summary>
    /// Internal method that configures PostgreSQL services with full customization options.
    /// Sets up mapping schema, data source, data options, and entity configurations.
    /// </summary>
    /// <typeparam name="TConnection">The DataConnection type</typeparam>
    /// <param name="container">The service container to configure</param>
    /// <param name="getCfg">Function to retrieve PostgreSQL configuration from service provider</param>
    /// <param name="configure">Action to configure the mapping schema</param>
    /// <param name="lifetime">The service lifetime for the connection</param>
    /// <returns>The configured service container for chaining</returns>
    private static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        Func<IServiceProvider, PostgreSqlConfiguration> getCfg,
        Action<IServiceProvider, MappingSchema> configure,
        ServiceLifetime lifetime
    )
        where TConnection : DataConnection
    {
        // mapping schema
        container
            .Add(sp =>
            {
                var mappingSchema = new MappingSchema();
                mappingSchema.ApplyConfigurations(sp).UseSnakeCaseColumns().UseJsonSupport(sp);
                configure(sp, mappingSchema);

                return new MappingSchemaContainer<TConnection>(mappingSchema);
            })
            .AsSelf()
            .Singleton();

        // data source
        container
            .Add(sp =>
            {
                var cfg = getCfg(sp);
                var logger = sp.Resolve<ILogger>();

                // configure data source and NodaTime
                var connectionString = cfg.ConnectionString;
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                dataSourceBuilder.UseNodaTime();
                var dataSource = dataSourceBuilder.Build();

                return new DataSourceContainer<TConnection>(dataSource, logger);
            })
            .AsSelf()
            .Singleton();

        container
            .Add(sp =>
            {
                var mappingSchema = sp.Resolve<MappingSchemaContainer<TConnection>>().Schema;
                var dataSource = sp.Resolve<DataSourceContainer<TConnection>>().DataSource;
                var connection = dataSource.CreateConnection();

                var options = new DataOptions()
                    .UseConnection(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v15), connection, true)
                    .UseMappingSchema(mappingSchema)
                    .UseLogging(sp);

                return new DataOptions<TConnection>(options);
            })
            .AsSelf()
            .In(lifetime);

        container.Add<TConnection>().AsSelf().In(lifetime);

        container.AddEntityConfigurations();

        return container;
    }
}

/// <summary>
/// Container for mapping schema associated with a specific connection type
/// </summary>
/// <typeparam name="TConnection">The connection type</typeparam>
/// <param name="Schema">The mapping schema</param>
// ReSharper disable once UnusedTypeParameter
file sealed record MappingSchemaContainer<TConnection>(MappingSchema Schema);

/// <summary>
/// Container for Npgsql data source with logging capabilities
/// </summary>
/// <typeparam name="TConnection">The connection type</typeparam>
// ReSharper disable once UnusedTypeParameter
file sealed record DataSourceContainer<TConnection> : IDisposable, ILogSubject
{
    /// <summary>
    /// Gets the Npgsql data source
    /// </summary>
    public NpgsqlDataSource DataSource { get; }

    /// <summary>
    /// Gets the logger instance
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the DataSourceContainer
    /// </summary>
    /// <param name="dataSource">The Npgsql data source</param>
    /// <param name="logger">The logger instance</param>
    public DataSourceContainer(NpgsqlDataSource dataSource, ILogger logger)
    {
        DataSource = dataSource;
        Logger = logger;
        this.Trace("created");
    }

    /// <summary>
    /// Disposes the data source and logs the disposal
    /// </summary>
    public void Dispose()
    {
        DataSource.Dispose();
        this.Trace("disposed");
    }
}
