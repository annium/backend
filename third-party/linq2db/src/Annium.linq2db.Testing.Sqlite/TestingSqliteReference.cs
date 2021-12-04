using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Annium.linq2db.Testing.Sqlite;

public class TestingSqliteReference
{
    private readonly string _dataSource = $"{Guid.NewGuid()}.db";

    public string ConnectionString => new SqliteConnectionStringBuilder
    {
        Mode = SqliteOpenMode.ReadWriteCreate,
        DataSource = _dataSource,
        Cache = SqliteCacheMode.Shared,
        ForeignKeys = true,
    }.ToString();

    public void Dispose()
    {
        if (File.Exists(_dataSource))
            File.Delete(_dataSource);
    }
}