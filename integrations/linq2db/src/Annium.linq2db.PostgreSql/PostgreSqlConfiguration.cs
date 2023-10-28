namespace Annium.linq2db.PostgreSql;

public record PostgreSqlConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string ConnectionString =>
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
