using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public class DatabaseMetadata
{
    public IReadOnlyDictionary<Type, TableMetadata> Tables { get; }

    public DatabaseMetadata(
        IReadOnlyDictionary<Type, TableMetadata> tables
    )
    {
        Tables = tables;
    }
}