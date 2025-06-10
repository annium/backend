using Annium.DbUp.Core;
using DbUp.Builder;

namespace Annium.DbUp.PostgreSql;

/// <summary>
/// PostgreSQL-specific implementation of the migration engine that configures DbUp for PostgreSQL databases
/// </summary>
public sealed class PostgresqlMigrationEngine : MigrationEngineBase<PostgresqlMigrationEngine>
{
    /// <summary>
    /// Initializes a new instance of the PostgreSQL migration engine
    /// </summary>
    /// <param name="initBuilder">The upgrade engine builder for initialization scripts</param>
    /// <param name="migrationsBuilder">The upgrade engine builder for migration scripts</param>
    /// <param name="schema">The database schema where the migration journal table will be created</param>
    public PostgresqlMigrationEngine(
        UpgradeEngineBuilder initBuilder,
        UpgradeEngineBuilder migrationsBuilder,
        string schema
    )
        : base(initBuilder, migrationsBuilder)
    {
        MigrationsBuilder.JournalToPostgresqlTable(schema, "db_migrations");
    }
}
