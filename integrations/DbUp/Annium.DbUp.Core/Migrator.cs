namespace Annium.DbUp.Core;

/// <summary>
/// Factory class for creating database migration engines for different database providers
/// </summary>
public class Migrator
{
    /// <summary>
    /// Gets the singleton instance of the migrator factory
    /// </summary>
    public static Migrator Instance { get; } = new();

    // public static IMigrationEngine ForSqlServer(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.SqlDatabase(connectionString));
    // public static IMigrationEngine ForMysql(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.MySqlDatabase(connectionString));
    // public static IMigrationEngine ForSqlite(string connectionString) => new MigrationEngineBase<>(DeployChanges.To.SQLiteDatabase(connectionString));

    /// <summary>
    /// Initializes a new instance of the Migrator class
    /// </summary>
    private Migrator() { }
}
