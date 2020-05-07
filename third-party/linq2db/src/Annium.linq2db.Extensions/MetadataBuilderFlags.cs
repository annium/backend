using System;

namespace Annium.linq2db.Extensions
{
    [Flags]
    public enum MetadataBuilderFlags
    {
        None = 0,
        IncludeMembersNotMarkedAsColumns = 1 << 0,
    }
}