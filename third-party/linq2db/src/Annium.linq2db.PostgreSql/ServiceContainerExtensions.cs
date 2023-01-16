using System;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Models;
using Annium.linq2db.PostgreSql;
using Annium.Logging.Abstractions;
using LinqToDB.Configuration;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Mapping;
using Npgsql;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container
    )
        where TConnection : DataConnectionBase, ILogSubject<TConnection>
    {
        return container.AddPostgreSql<TConnection>(
            sp => sp.Resolve<PostgreSqlConfiguration>(),
            (_, _) => { }
        );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        Action<IServiceProvider, MappingSchema> configure
    )
        where TConnection : DataConnectionBase, ILogSubject<TConnection>
    {
        return container
            .AddPostgreSql<TConnection>(
                sp => sp.Resolve<PostgreSqlConfiguration>(),
                configure
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        PostgreSqlConfiguration cfg
    )
        where TConnection : DataConnectionBase, ILogSubject<TConnection>
    {
        return container
            .AddPostgreSql<TConnection>(
                _ => cfg,
                (_, _) => { }
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        PostgreSqlConfiguration cfg,
        Action<IServiceProvider, MappingSchema> configure
    )
        where TConnection : DataConnectionBase, ILogSubject<TConnection>
    {
        return container
            .AddPostgreSql<TConnection>(
                _ => cfg,
                configure
            );
    }

    private static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        Func<IServiceProvider, PostgreSqlConfiguration> getCfg,
        Action<IServiceProvider, MappingSchema> configure
    )
        where TConnection : DataConnectionBase, ILogSubject<TConnection>
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

        container.Add<TConnection>().AsSelf().Transient();

        container.AddEntityConfigurations();

        return container;
    }
}