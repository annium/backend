using System.Collections.Generic;

namespace Annium.linq2db.Extensions.Configuration.Metadata;

public class DbMetadata
{
    public IReadOnlyCollection<TableMetadata> Tables { get; }

    public DbMetadata(
        IReadOnlyCollection<TableMetadata> tables
    )
    {
        Tables = tables;
    }
}