using System;
using System.Collections.Generic;

namespace Annium.linq2db.Extensions.Configuration.Metadata;

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