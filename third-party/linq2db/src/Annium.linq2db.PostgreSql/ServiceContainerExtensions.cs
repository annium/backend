using System;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Models;
using Annium.linq2db.PostgreSql;
using Annium.Logging.Abstractions;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;

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
            builder.UseConnectionString(ProviderName.PostgreSQL95, string.Join(
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
                "MinPoolSize=5",
                "MaxPoolSize=1000"
            ));

            var mappingSchema = new MappingSchema();
            mappingSchema
                .ApplyConfigurations(sp)
                .UseSnakeCaseColumns()
                .UseJsonSupport(sp);
            configure(sp, mappingSchema);
            builder.UseMappingSchema(mappingSchema);
            builder.UseLogging<TConnection>(sp);

            var options = builder.Build();

            return new Config<TConnection>(options);
        }).AsSelf().Singleton();

        container.Add<TConnection>().AsSelf().Transient();

        container.AddEntityConfigurations();

        return container;
    }
}