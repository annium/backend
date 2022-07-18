using System;

namespace Annium.linq2db.Extensions.Configuration;

[Flags]
public enum MetadataBuilderFlags
{
    None = 0,
    IncludeMembersNotMarkedAsColumns = 1 << 0,
}