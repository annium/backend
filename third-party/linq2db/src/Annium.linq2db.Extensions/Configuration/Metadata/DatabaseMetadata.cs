using System.Collections.Generic;

namespace Annium.linq2db.Extensions.Configuration.Metadata;

public class DatabaseMetadata
{
    public IReadOnlyCollection<TableMetadata> Tables { get; }

    public DatabaseMetadata(
        IReadOnlyCollection<TableMetadata> tables
    )
    {
        Tables = tables;
    }
}