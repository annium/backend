using System;
using System.Reflection;
using System.Threading.Tasks;
using DbUp;
using Testcontainers.PostgreSql;

namespace Annium.linq2db.PostgreSql.Tests.Db;

public class Database
{
    public PostgreSqlConfiguration Config { get; } =
        new()
        {
            Database = "db",
            User = "postgres",
            Password = "postgres",
        };

    private readonly PostgreSqlContainer _db;

    public Database()
    {
        _db = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase(Config.Database)
            .WithUsername(Config.User)
            .WithPassword(Config.Password)
            .Build();
    }

    public async Task InitAsync()
    {
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
    }
}
