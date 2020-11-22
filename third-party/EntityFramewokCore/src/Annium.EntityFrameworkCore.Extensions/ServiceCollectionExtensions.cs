using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddEntityFrameworkSqliteInMemory<TContext>(
            this IServiceContainer container,
            IServiceProvider provider
        )
            where TContext : DbContext
        {
            return container.AddEntityFrameworkSqliteInMemory<TContext>(provider.Resolve<SqliteConnection>());
        }

        public static IServiceContainer AddEntityFrameworkSqliteInMemory<TContext>(
            this IServiceContainer container
        )
            where TContext : DbContext
        {
            return container.AddEntityFrameworkSqliteInMemory<TContext>(new SqliteConnection("Data Source=:memory:"));
        }

        private static IServiceContainer AddEntityFrameworkSqliteInMemory<TContext>(
            this IServiceContainer container,
            SqliteConnection cn
        )
            where TContext : DbContext
        {
            cn.Open();

            // register context itself
            container.Collection
                .AddDbContext<TContext>(builder =>
                {
                    var opts = builder.UseSqlite(cn).Options;
                    using var ctx = (DbContext) Activator.CreateInstance(typeof(TContext), opts)!;
                    ctx.Database.EnsureCreated();
                });

            return container;
        }
    }
}