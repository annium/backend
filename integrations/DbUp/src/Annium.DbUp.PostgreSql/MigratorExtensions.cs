using Annium.DbUp.Core;
using DbUp;

namespace Annium.DbUp.PostgreSql;

/// <summary>
/// Extension methods for the Migrator class to support PostgreSQL database migrations
/// </summary>
public static class MigratorExtensions
{
    /// <summary>
    /// Creates a PostgreSQL migration engine configured for the specified connection string and schema
    /// </summary>
    /// <param name="_">The migrator instance (not used, for extension method syntax)</param>
    /// <param name="connectionString">The PostgreSQL connection string</param>
    /// <param name="schema">The database schema where the migration journal table will be created</param>
    /// <returns>A configured PostgreSQL migration engine</returns>
    public static PostgresqlMigrationEngine ForPostgresql(this Migrator _, string connectionString, string schema) =>
        new(
            DeployChanges.To.PostgresqlDatabase(connectionString),
            DeployChanges.To.PostgresqlDatabase(connectionString),
            schema
        );
}
