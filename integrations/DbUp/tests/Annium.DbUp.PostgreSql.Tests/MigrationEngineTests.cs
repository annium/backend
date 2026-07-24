using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Annium.DbUp.Core;
using Annium.Testing;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Annium.DbUp.PostgreSql.Tests;

/// <summary>
/// Integration tests for the DbUp PostgreSQL migration engine, exercised end-to-end against a real
/// PostgreSQL instance started via Testcontainers. Each test gets a fresh container (a new class
/// instance is created per test), so migration-journal state is isolated between tests.
/// </summary>
public class MigrationEngineTests : IAsyncLifetime
{
    /// <summary>
    /// The non-default schema the migration journal table is expected to be routed into.
    /// </summary>
    private const string Schema = "app";

    /// <summary>
    /// The per-test PostgreSQL container.
    /// </summary>
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    /// <summary>
    /// Temp script folders created by <see cref="CreateScriptsDir"/>, removed on disposal.
    /// </summary>
    private readonly List<string> _tempDirs = new();

    /// <summary>
    /// Starts the container and waits until it accepts connections before any test runs.
    /// </summary>
    /// <returns>A task that completes once the database is ready.</returns>
    public async ValueTask InitializeAsync()
    {
        await _db.StartAsync();
        await WaitForReadyAsync();
    }

    /// <summary>
    /// Stops and removes the container.
    /// </summary>
    /// <returns>A task that completes once the container is disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        foreach (var dir in _tempDirs)
        {
            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // best-effort cleanup — leftover temp folders must never fail a test
            }
        }

        await _db.DisposeAsync();
    }

    /// <summary>
    /// Assembly-embedded init + migration scripts run end-to-end: the migration is applied and the
    /// migration journal table is created in the caller-supplied schema (not the default public schema).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Execute_AssemblyScripts_AppliesMigrationsAndJournalsInConfiguredSchema()
    {
        // act — init creates the schema, the migration creates app.items, journal → app.db_migrations
        Migrator
            .Instance.ForPostgresql(_db.GetConnectionString(), Schema)
            .WithScriptsFromAssembly(Assembly.GetExecutingAssembly())
            .Execute();

        // assert — the migration script was applied
        var itemsCount = await CountInSchemaAsync("items");
        itemsCount.Is(1L);

        // assert — the migration journal table lives in the configured schema, not public
        var journalSchema = await TextAsync(
            "select table_schema from information_schema.tables where table_name = 'db_migrations'"
        );
        journalSchema.Is(Schema);
    }

    /// <summary>
    /// A failing migration script surfaces as an <see cref="ApplicationException"/> from Execute().
    /// </summary>
    [Fact]
    public void Execute_FailingScript_ThrowsApplicationException()
    {
        // arrange — a valid init (creates the schema) followed by a deliberately invalid migration
        var dir = CreateScriptsDir(init: "create schema if not exists app;", migration: "this is not valid sql;");
        var engine = Migrator.Instance.ForPostgresql(_db.GetConnectionString(), Schema).WithScriptsFromDirectory(dir);

        // act + assert — the message names the failing script (ErrorScript.Name) and surfaces the SQL error
        var exception = Wrap.It(() => engine.Execute()).Throws<ApplicationException>();
        exception.Message.Contains("001_migration").IsTrue();
        exception.Message.Contains("syntax error").IsTrue();
    }

    /// <summary>
    /// A failing INIT script short-circuits the run: migrations must not execute after init fails. This pins
    /// Execute()'s own composition (ExecuteBuilder(Init) then ExecuteBuilder(Migrations)), not DbUp's behavior.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Execute_FailingInitScript_DoesNotRunMigrations()
    {
        // arrange — the init script fails; the migration would otherwise create a distinctive table
        var dir = CreateScriptsDir(
            init: "this is not valid init sql;",
            migration: "create schema if not exists app;\ncreate table app.should_not_exist (id int not null);"
        );
        var engine = Migrator.Instance.ForPostgresql(_db.GetConnectionString(), Schema).WithScriptsFromDirectory(dir);

        // act — init fails, so Execute throws before the migration phase runs
        Wrap.It(() => engine.Execute()).Throws<ApplicationException>();

        // assert — the migration never ran: its table (and its schema) were never created
        var count = await CountInSchemaAsync("should_not_exist");
        count.Is(0L);
    }

    /// <summary>
    /// WithTransactionPerScript (configured in the engine constructor) wraps each script in a transaction:
    /// a script that creates a table and then fails must leave NO trace — the earlier statement is rolled
    /// back. This pins the constructor's per-script-transaction choice (a mutant dropping it would leave the
    /// table behind).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Execute_FailingScriptMidTransaction_RollsBackEarlierStatements()
    {
        // arrange — one migration script: create a table, then an invalid statement in the same script
        var dir = CreateScriptsDir(
            init: "create schema if not exists app;",
            migration: "create table app.rollback_probe (id int not null);\nthis is not valid sql;"
        );
        var engine = Migrator.Instance.ForPostgresql(_db.GetConnectionString(), Schema).WithScriptsFromDirectory(dir);

        // act — the invalid second statement fails the script
        Wrap.It(() => engine.Execute()).Throws<ApplicationException>();

        // assert — the per-script transaction rolled the whole script back: the table must not exist
        var probeCount = await CountInSchemaAsync("rollback_probe");
        probeCount.Is(0L);
    }

    /// <summary>
    /// Filesystem (directory) script discovery runs end-to-end and applies the migration, pinning the
    /// WithScriptsFromDirectory path independently of the assembly-embedded path.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Execute_DirectoryScripts_AppliesMigrations()
    {
        // arrange
        var dir = CreateScriptsDir(
            init: "create schema if not exists app;",
            migration: "create table app.widgets (id int not null, constraint pk_widgets primary key (id));"
        );

        // act
        Migrator.Instance.ForPostgresql(_db.GetConnectionString(), Schema).WithScriptsFromDirectory(dir).Execute();

        // assert
        var widgetsCount = await CountInSchemaAsync("widgets");
        widgetsCount.Is(1L);
    }

    /// <summary>
    /// Script variables set via WithVariable (single) and WithVariables (dictionary) are substituted in
    /// BOTH the init and the migration scripts — pinning the documented "available to both initialization
    /// and migration scripts" invariant. Each variable is referenced in both scripts: if it failed to reach
    /// the init builder the init script would throw (undefined variable), and the migration-side assertions
    /// prove it also reached the migration builder.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Execute_WithVariables_SubstitutesInBothInitAndMigrationScripts()
    {
        // arrange — $Single$ (via WithVariable) and $Multi$ (via WithVariables) referenced in both scripts
        var dir = CreateScriptsDir(
            init: "create schema if not exists app;\n"
                + "create table app.$Single$_init (id int not null);\n"
                + "create table app.$Multi$_init (id int not null);",
            migration: "create table app.$Single$_mig (id int not null);\n"
                + "create table app.$Multi$_mig (id int not null);"
        );

        // act
        Migrator
            .Instance.ForPostgresql(_db.GetConnectionString(), Schema)
            .WithScriptsFromDirectory(dir)
            .WithVariable("Single", "single")
            .WithVariables(new Dictionary<string, string> { ["Multi"] = "multi" })
            .Execute();

        // assert — the migration builder received both variables (and Execute not throwing proves the init
        // builder received them too, since the init script references both)
        var singleCount = await CountInSchemaAsync("single_mig");
        singleCount.Is(1L);

        var multiCount = await CountInSchemaAsync("multi_mig");
        multiCount.Is(1L);
    }

    /// <summary>
    /// Retries opening a connection until PostgreSQL accepts it — the container readiness probe can pass
    /// while the server is still finishing first-boot startup.
    /// </summary>
    /// <returns>A task that completes once a connection succeeds.</returns>
    private async Task WaitForReadyAsync()
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var connection = await OpenConnectionAsync();
                return;
            }
            catch when (attempt < 10)
            {
                await Task.Delay(500);
            }
        }
    }

    /// <summary>
    /// Opens a new connection to the test database.
    /// </summary>
    /// <returns>An open <see cref="NpgsqlConnection"/>.</returns>
    private async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var connection = new NpgsqlConnection(_db.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>
    /// Runs a scalar query and returns the raw result.
    /// </summary>
    /// <param name="sql">The SQL to execute.</param>
    /// <returns>The scalar result, or null.</returns>
    private async Task<object?> ScalarAsync(string sql)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return await command.ExecuteScalarAsync();
    }

    /// <summary>
    /// Runs a scalar <c>count(*)</c> query and returns the count.
    /// </summary>
    /// <param name="sql">The SQL to execute.</param>
    /// <returns>The scalar count.</returns>
    private async Task<long> CountAsync(string sql) => (long)(await ScalarAsync(sql)).NotNull();

    /// <summary>
    /// Counts tables named <paramref name="tableName"/> in the configured <see cref="Schema"/>.
    /// </summary>
    /// <param name="tableName">The unqualified table name to look for.</param>
    /// <returns>The number of matching tables (0 or 1).</returns>
    private Task<long> CountInSchemaAsync(string tableName) =>
        CountAsync(
            $"select count(*) from information_schema.tables where table_schema = '{Schema}' and table_name = '{tableName}'"
        );

    /// <summary>
    /// Runs a scalar query returning a single text value.
    /// </summary>
    /// <param name="sql">The SQL to execute.</param>
    /// <returns>The scalar text value, or null if absent.</returns>
    private async Task<string?> TextAsync(string sql) => (string?)await ScalarAsync(sql);

    /// <summary>
    /// Writes a temporary Scripts/Init + Scripts/Migrations folder tree for WithScriptsFromDirectory.
    /// </summary>
    /// <param name="init">The single init script body.</param>
    /// <param name="migration">The single migration script body.</param>
    /// <returns>The root folder to pass to WithScriptsFromDirectory.</returns>
    private string CreateScriptsDir(string init, string migration)
    {
        var root = Path.Combine(Path.GetTempPath(), "annium-dbup-test-" + Guid.NewGuid().ToString("N"));
        var initDir = Path.Combine(root, "Scripts", "Init");
        var migrationsDir = Path.Combine(root, "Scripts", "Migrations");
        Directory.CreateDirectory(initDir);
        Directory.CreateDirectory(migrationsDir);
        File.WriteAllText(Path.Combine(initDir, "001_init.sql"), init);
        File.WriteAllText(Path.Combine(migrationsDir, "001_migration.sql"), migration);
        _tempDirs.Add(root);

        return root;
    }
}
