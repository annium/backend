using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DbUp;
using Testcontainers.PostgreSql;

namespace Annium.linq2db.PostgreSql.Tests.Db;

public static class Database
{
    public static PostgreSqlConfiguration Config { get; } =
        new()
        {
            Database = "db",
            User = "postgres",
            Password = "postgres",
        };

    private static readonly PostgreSqlContainer Db;
    private static readonly TaskCompletionSource InitTcs = new();
    private static volatile int _refs;

    static Database()
    {
        Db = new PostgreSqlBuilder()
            .WithImage("registry.annium.com/postgres:16.0-alpine")
            .WithDatabase(Config.Database)
            .WithUsername(Config.User)
            .WithPassword(Config.Password)
            .Build();
    }

    public static async Task AcquireAsync()
    {
        if (Interlocked.Increment(ref _refs) > 1)
        {
            await InitTcs.Task;
            return;
        }

        await Db.StartAsync();
        Config.Host = Db.Hostname;
        Config.Port = Db.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);
        var result = DeployChanges
            .To.PostgresqlDatabase(Db.GetConnectionString())
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), x => x.Contains(".Migrations."))
            .WithTransactionPerScript()
            .LogToConsole()
            .Build()
            .PerformUpgrade();
        if (!result.Successful)
            throw new ApplicationException($"{result.ErrorScript}: {result.Error}");
        InitTcs.SetResult();
    }
}
