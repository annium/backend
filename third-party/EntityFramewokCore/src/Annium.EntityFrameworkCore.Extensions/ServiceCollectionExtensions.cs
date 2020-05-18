using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkSqliteInMemory<TContext>(
            this IServiceCollection services,
            IServiceProvider provider
        )
            where TContext : DbContext
        {
            return services.AddEntityFrameworkSqliteInMemory<TContext>(provider.GetRequiredService<SqliteConnection>());
        }

        public static IServiceCollection AddEntityFrameworkSqliteInMemory<TContext>(
            this IServiceCollection services
        )
            where TContext : DbContext
        {
            return services.AddEntityFrameworkSqliteInMemory<TContext>(new SqliteConnection("Data Source=:memory:"));
        }

        private static IServiceCollection AddEntityFrameworkSqliteInMemory<TContext>(
            this IServiceCollection services,
            SqliteConnection cn
        )
            where TContext : DbContext
        {
            cn.Open();

            // register context itself
            return services
                .AddDbContext<TContext>(builder =>
                {
                    var opts = builder.UseSqlite(cn).Options;
                    using var ctx = (DbContext) Activator.CreateInstance(typeof(TContext), opts)!;
                    ctx.Database.EnsureCreated();
                });
        }
    }
}