using System;
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
                .AddPostgreSql<TConnection>(cfg, mappingSchema => { });
        }

        public static IServiceCollection AddPostgreSql<TConnection>(
            this IServiceCollection services,
            IPostgreSqlConfiguration cfg,
            Action<MappingSchema> configure
        )
            where TConnection : DataConnectionBase
        {
            var mappingSchema = new MappingSchema();
            mappingSchema.GetMappingBuilder()
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