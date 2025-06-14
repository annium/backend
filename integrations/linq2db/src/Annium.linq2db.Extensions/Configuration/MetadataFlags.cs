using System;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Flags for controlling metadata generation behavior
/// </summary>
[Flags]
public enum MetadataFlags
{
    /// <summary>
    /// No special metadata flags
    /// </summary>
    None = 0,

    /// <summary>
    /// Include members that are not explicitly marked as columns
    /// </summary>
    IncludeMembersNotMarkedAsColumns = 1 << 0,
}
