namespace Annium.DbUp.Core;

public class Migrator
{
    public static Migrator Instance { get; } = new();

    // public static IMigrationEngine ForSqlServer(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.SqlDatabase(connectionString));
    // public static IMigrationEngine ForMysql(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.MySqlDatabase(connectionString));
    // public static IMigrationEngine ForSqlite(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.SQLiteDatabase(connectionString));

    private Migrator() { }
}
