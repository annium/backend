using System;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Models;
using Annium.linq2db.PostgreSql;
using Annium.Logging.Abstractions;
using LinqToDB.Configuration;
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
        where TConnection : DataConnection, ILogSubject<TConnection>
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
        where TConnection : DataConnection, ILogSubject<TConnection>
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
        where TConnection : DataConnection, ILogSubject<TConnection>
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
        where TConnection : DataConnection, ILogSubject<TConnection>
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
        where TConnection : DataConnection, ILogSubject<TConnection>
    {
        container.Add(sp =>
        {
            var cfg = getCfg(sp);
            var builder = new LinqToDBConnectionOptionsBuilder();

            // configure data source and NodaTime
            var connectionString = cfg.ConnectionString;
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseNodaTime();
            var dataSource = dataSourceBuilder.Build();
            builder.UseConnectionFactory(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v15), dataSource.CreateConnection);

            // configure mapping
            var mappingSchema = new MappingSchema();
            mappingSchema
                .ApplyConfigurations(sp)
                .UseSnakeCaseColumns()
                .UseJsonSupport(sp);
            configure(sp, mappingSchema);
            builder.UseMappingSchema(mappingSchema);

            // add logging
            builder.UseLogging<TConnection>(sp);

            var options = builder.Build();

            return new Config<TConnection>(options);
        }).AsSelf().Singleton();

        container.Add<TConnection>().AsSelf().In(lifetime);

        container.AddEntityConfigurations();

        return container;
    }
}