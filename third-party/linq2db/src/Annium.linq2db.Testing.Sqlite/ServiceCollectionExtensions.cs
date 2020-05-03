using System;
using System.Reflection;
using Annium.linq2db.Extensions;
using Annium.linq2db.Testing.Sqlite;
using FluentMigrator.Runner;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTestingSqlite<TConnection>(
            this IServiceCollection services,
            Assembly migrationsAssembly
        )
            where TConnection : DataConnectionBase
        {
            return services
                .AddTestingSqlite<TConnection>(
                    Assembly.GetCallingAssembly(),
                    migrationsAssembly,
                    mappingSchema => { }
                );
        }

        public static IServiceCollection AddTestingSqlite<TConnection>(
            this IServiceCollection services,
            Assembly migrationsAssembly,
            Action<MappingSchema> configure
        )
            where TConnection : DataConnectionBase
        {
            return services
                .AddTestingSqlite<TConnection>(
                    Assembly.GetCallingAssembly(),
                    migrationsAssembly,
                    configure
                );
        }

        public static IServiceCollection AddTestingSqlite<TConnection>(
            this IServiceCollection services,
            Assembly configurationsAssembly,
            Assembly migrationsAssembly
        )
            where TConnection : DataConnectionBase
        {
            return services
                .AddTestingSqlite<TConnection>(
                    configurationsAssembly,
                    migrationsAssembly,
                    mappingSchema => { }
                );
        }

        public static IServiceCollection AddTestingSqlite<TConnection>(
            this IServiceCollection services,
            Assembly configurationsAssembly,
            Assembly migrationsAssembly,
            Action<MappingSchema> configure
        )
            where TConnection : DataConnectionBase
        {
            var mappingSchema = new MappingSchema();
            mappingSchema.GetMappingBuilder(configurationsAssembly)
                .ApplyConfigurations()
                .CamelCaseColumns();
            configure?.Invoke(mappingSchema);

            services.AddSingleton(sp =>
            {
                var testingReference = new TestingSqliteReference();

                var migrationRunner = CreateMigrationRunner(testingReference.ConnectionString, migrationsAssembly);
                migrationRunner.MigrateUp();

                return testingReference;
            });


            services.AddScoped(sp => (TConnection) Activator.CreateInstance(
                typeof(TConnection),
                ProviderName.SQLiteMS,
                sp.GetRequiredService<TestingSqliteReference>().ConnectionString,
                mappingSchema
            )!);
            Configuration.Linq.AllowMultipleQuery = true;

            return services;
        }

        private static IMigrationRunner CreateMigrationRunner(string connectionString, Assembly migrationsAssembly)
        {
            var localServices = new ServiceCollection();
            localServices
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(migrationsAssembly).For.Migrations()
                );

            return localServices.BuildServiceProvider().GetRequiredService<IMigrationRunner>();
        }
    }
}