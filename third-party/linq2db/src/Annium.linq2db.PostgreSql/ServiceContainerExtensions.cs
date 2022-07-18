using System;
using System.Reflection;
using Annium.linq2db.Extensions;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Models;
using Annium.linq2db.PostgreSql;
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
        where TConnection : DataConnectionBase
    {
        return container
            .AddPostgreSql<TConnection>(
                Assembly.GetCallingAssembly(),
                cfg,
                (_, _) => { }
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        IPostgreSqlConfiguration cfg,
        Action<IServiceProvider, IMappingBuilder> configure
    )
        where TConnection : DataConnectionBase
    {
        return container
            .AddPostgreSql<TConnection>(
                Assembly.GetCallingAssembly(),
                cfg,
                configure
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        Assembly configurationsAssembly,
        IPostgreSqlConfiguration cfg
    )
        where TConnection : DataConnectionBase
    {
        return container
            .AddPostgreSql<TConnection>(
                configurationsAssembly,
                cfg,
                (_, _) => { }
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        Assembly configurationsAssembly,
        IPostgreSqlConfiguration cfg,
        Action<IServiceProvider, IMappingBuilder> configure
    )
        where TConnection : DataConnectionBase
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
                "Trust Server Certificate=true"
            ));

            var mappingSchema = new MappingSchema();
            var mappingBuilder = mappingSchema.GetMappingBuilder(configurationsAssembly);
            mappingBuilder
                .ApplyConfigurations()
                .UseSnakeCaseColumns()
                .UseJsonSupport(sp);
            configure(sp, mappingBuilder);
            builder.UseMappingSchema(mappingSchema);
            builder.UseLogging<TConnection>(sp);

            var options = builder.Build();

            return (TConnection) Activator.CreateInstance(typeof(TConnection), options)!;
        }).AsSelf().Scoped();

        container.AddRepositories();

        return container;
    }
}