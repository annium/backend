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
        this IServiceContainer container,
        IPostgreSqlConfiguration cfg
    )
        where TConnection : DataConnectionBase, ILogSubject<TConnection>
    {
        return container
            .AddPostgreSql<TConnection>(
                cfg,
                (_, _) => { }
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        IPostgreSqlConfiguration cfg,
        Action<IServiceProvider, MappingSchema> configure
    )
        where TConnection : DataConnectionBase, ILogSubject<TConnection>
    {
        container.Add(sp =>
        {
            var builder = new LinqToDBConnectionOptionsBuilder();

            // connection string setup
            var connectionString = string.Join(
                ';',
                $"Host={cfg.Host}",
                $"Port={cfg.Port}",
                $"Database={cfg.Database}",
                $"Username={cfg.User}",
                $"Password={cfg.Password}",
                "SSL Mode=Prefer",
                "Trust Server Certificate=true",
                "Keepalive=30",
                "Tcp Keepalive=true",
                "ConnectionLifetime=180",
                "MinPoolSize=5",
                "MaxPoolSize=1000"
            );
            builder.UsePostgreSQL(connectionString, PostgreSQLVersion.v15);

            // configure data source and NodaTime
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