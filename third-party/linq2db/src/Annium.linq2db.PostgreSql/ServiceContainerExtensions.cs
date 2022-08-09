using System;
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
                cfg,
                (_, _) => { }
            );
    }

    public static IServiceContainer AddPostgreSql<TConnection>(
        this IServiceContainer container,
        IPostgreSqlConfiguration cfg,
        Action<IServiceProvider, MappingSchema> configure
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