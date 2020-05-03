using System;
using System.Reflection;
using Annium.linq2db.Extensions;
using Annium.linq2db.PostgreSql;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPostgreSql<TConnection>(
            this IServiceCollection services,
            IPostgreSqlConfiguration cfg
        )
            where TConnection : DataConnectionBase
        {
            return services
                .AddPostgreSql<TConnection>(
                    Assembly.GetCallingAssembly(),
                    cfg,
                    mappingSchema => { }
                );
        }

        public static IServiceCollection AddPostgreSql<TConnection>(
            this IServiceCollection services,
            IPostgreSqlConfiguration cfg,
            Action<MappingSchema> configure
        )
            where TConnection : DataConnectionBase
        {
            return services
                .AddPostgreSql<TConnection>(
                    Assembly.GetCallingAssembly(),
                    cfg,
                    configure
                );
        }

        public static IServiceCollection AddPostgreSql<TConnection>(
            this IServiceCollection services,
            Assembly configurationsAssembly,
            IPostgreSqlConfiguration cfg
        )
            where TConnection : DataConnectionBase
        {
            return services
                .AddPostgreSql<TConnection>(
                    configurationsAssembly,
                    cfg,
                    mappingSchema => { }
                );
        }

        public static IServiceCollection AddPostgreSql<TConnection>(
            this IServiceCollection services,
            Assembly configurationsAssembly,
            IPostgreSqlConfiguration cfg,
            Action<MappingSchema> configure
        )
            where TConnection : DataConnectionBase
        {
            var mappingSchema = new MappingSchema();
            mappingSchema.GetMappingBuilder(configurationsAssembly)
                .ApplyConfigurations()
                .CamelCaseColumns();
            configure(mappingSchema);

            services.AddScoped(sp => (TConnection) Activator.CreateInstance(
                typeof(TConnection),
                ProviderName.PostgreSQL95,
                string.Join(
                    ';',
                    $"Host={cfg.Host}",
                    $"Port={cfg.Port}",
                    $"Database={cfg.Database}",
                    $"Username={cfg.User}",
                    $"Password={cfg.Password}",
                    "SSL Mode=Prefer",
                    "Trust Server Certificate=true"
                ),
                mappingSchema
            )!);
            Configuration.Linq.AllowMultipleQuery = true;

            return services;
        }
    }
}