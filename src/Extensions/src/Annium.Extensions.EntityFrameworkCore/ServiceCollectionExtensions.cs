using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.EntityFrameworkCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEntityFrameworkSqliteInMemory<TContext>(this IServiceCollection services) where TContext : DbContext
        {
            var cn = new SqliteConnection("Data Source=:memory:");
            cn.Open();
            // register context itself
            services
                .AddEntityFrameworkSqlite()
                .AddDbContext<TContext>(builder =>
                {
                    var opts = builder.UseSqlite(cn).Options;
                    using(var ctx = Activator.CreateInstance(typeof(TContext), opts) as DbContext) ctx.Database.EnsureCreated();
                });
        }
    }
}