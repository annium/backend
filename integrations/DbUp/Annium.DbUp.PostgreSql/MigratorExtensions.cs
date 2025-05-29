using Annium.DbUp.Core;
using DbUp;

namespace Annium.DbUp.PostgreSql;

public static class MigratorExtensions
{
    public static PostgresqlMigrationEngine ForPostgresql(this Migrator _, string connectionString, string schema) =>
        new(
            DeployChanges.To.PostgresqlDatabase(connectionString),
            DeployChanges.To.PostgresqlDatabase(connectionString),
            schema
        );
}
