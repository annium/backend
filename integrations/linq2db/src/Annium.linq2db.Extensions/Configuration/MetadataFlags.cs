using System;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

[Flags]
public enum MetadataFlags
{
    None = 0,
    IncludeMembersNotMarkedAsColumns = 1 << 0,
}
