using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            var loggerFactory = logQueries ? CreateLoggerFactory() : null;

            // register context itself
            services
                .AddEntityFrameworkSqlite()
                .AddDbContext<TContext>(builder =>
                {
                    if (logQueries)
                        builder.UseLoggerFactory(loggerFactory);

                    var opts = builder.UseSqlite(cn).Options;
                    using(var ctx = Activator.CreateInstance(typeof(TContext), opts) as DbContext) ctx.Database.EnsureCreated();
                });
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
                builder
                .AddConsole()
                .AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information));

            return serviceCollection.BuildServiceProvider()
                .GetService<ILoggerFactory>();
        }
    }
}