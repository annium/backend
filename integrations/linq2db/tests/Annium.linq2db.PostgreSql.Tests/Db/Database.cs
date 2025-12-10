using System;
using System.Reflection;
using System.Threading.Tasks;
using DbUp;
using Testcontainers.PostgreSql;

namespace Annium.linq2db.PostgreSql.Tests.Db;

/// <summary>
/// Test database setup for PostgreSQL integration tests using Testcontainers
/// </summary>
public class Database
{
    /// <summary>
    /// Gets the PostgreSQL configuration for test connections
    /// </summary>
    public PostgreSqlConfiguration Config { get; } =
        new()
        {
            Database = "db",
            User = "postgres",
            Password = "postgres",
        };

    /// <summary>
    /// PostgreSQL container instance for testing
    /// </summary>
    private readonly PostgreSqlContainer _db;

    /// <summary>
    /// Initializes a new instance of the Database class with PostgreSQL container configuration
    /// </summary>
    public Database()
    {
        _db = new PostgreSqlBuilder()
            .WithImage("postgres:18-alpine")
            .WithDatabase(Config.Database)
            .WithUsername(Config.User)
            .WithPassword(Config.Password)
            .Build();
    }

    /// <summary>
    /// Initializes the PostgreSQL test database by starting the container and running migrations
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation</returns>
    /// <exception cref="ApplicationException">Thrown when database migration fails</exception>
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
