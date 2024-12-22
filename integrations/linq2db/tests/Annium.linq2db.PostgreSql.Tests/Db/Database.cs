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

    private static readonly PostgreSqlContainer _db;
    private static readonly TaskCompletionSource _initTcs = new();
    private static volatile int _refs;

    static Database()
    {
        _db = new PostgreSqlBuilder()
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
            await _initTcs.Task;
            return;
        }

        await _db.StartAsync();
        Config.Host = _db.Hostname;
        Config.Port = _db.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);
        var result = DeployChanges
            .To.PostgresqlDatabase(_db.GetConnectionString())
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), x => x.Contains(".Migrations."))
            .WithTransactionPerScript()
            .LogToConsole()
            .Build()
            .PerformUpgrade();
        if (!result.Successful)
            throw new ApplicationException($"{result.ErrorScript}: {result.Error}");
        _initTcs.SetResult();
    }
}
