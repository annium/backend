namespace Annium.linq2db.PostgreSql;

/// <summary>
/// Configuration settings for PostgreSQL database connections
/// </summary>
public record PostgreSqlConfiguration
{
    /// <summary>
    /// Gets or sets the PostgreSQL server hostname or IP address
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PostgreSQL server port number
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the name of the database to connect to
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username for database authentication
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for database authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets the PostgreSQL connection string built from the configuration properties
    /// </summary>
    public virtual string ConnectionString =>
        string.Join(
            ';',
            $"Host={Host}",
            $"Port={Port}",
            $"Database={Database}",
            $"Username={User}",
            $"Password={Password}",
            "SSL Mode=Prefer",
            "Trust Server Certificate=true",
            "Keepalive=30",
            "Connection Lifetime=180",
            "Pooling=true",
            "Minimum Pool Size=100",
            "Maximum Pool Size=1000"
        );
}
