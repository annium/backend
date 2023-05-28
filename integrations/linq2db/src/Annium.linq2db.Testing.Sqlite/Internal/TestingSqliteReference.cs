using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Annium.linq2db.Testing.Sqlite.Internal;

internal class TestingSqliteReference : IDisposable
{
    public string ConnectionString { get; }
    private readonly string _dataSource;

    public TestingSqliteReference()
    {
        _dataSource = $"{Guid.NewGuid()}.db";
        ConnectionString = new SqliteConnectionStringBuilder
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            DataSource = _dataSource,
            Cache = SqliteCacheMode.Shared,
            ForeignKeys = true,
        }.ToString();
    }

    public void Dispose()
    {
        if (File.Exists(_dataSource))
            File.Delete(_dataSource);
    }

    public override string ToString() => ConnectionString;
}