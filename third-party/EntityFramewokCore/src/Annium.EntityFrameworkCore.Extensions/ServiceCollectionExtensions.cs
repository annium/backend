using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEntityFrameworkSqliteInMemory<TContext>(
            this IServiceCollection services,
            bool logQueries = false
        )
            where TContext : DbContext
        {
            var cn = new SqliteConnection("Data Source=:memory:");
            cn.Open();

            // register context itself
            services
                .AddDbContext<TContext>(builder =>
                {
                    var opts = builder.UseSqlite(cn).Options;
                    using var ctx = (DbContext) Activator.CreateInstance(typeof(TContext), opts)!;
                    ctx.Database.EnsureCreated();
                });
        }
    }
}