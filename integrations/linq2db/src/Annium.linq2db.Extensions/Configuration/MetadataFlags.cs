using System;

namespace Annium.linq2db.Extensions.Configuration;

[Flags]
public enum MetadataFlags
{
    None = 0,
    IncludeMembersNotMarkedAsColumns = 1 << 0,
}
