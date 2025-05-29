using Annium.DbUp.Core;
using DbUp.Builder;

namespace Annium.DbUp.PostgreSql;

public sealed class PostgresqlMigrationEngine : MigrationEngineBase<PostgresqlMigrationEngine>
{
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
