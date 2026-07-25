namespace Annium.linq2db.Tests.Lib.Db.Models;

/// <summary>
/// A minimal entity with no primary key, used to exercise the empty-primary-key edge of
/// <c>TableSaveExtensions.UpdateAsync</c> (its predicate builder aggregates over the primary-key
/// columns and throws when there are none). Intentionally not backed by a migration — no test
/// queries it; it exists only in the mapping schema so metadata resolution succeeds.
/// </summary>
public sealed record NoPkEntity
{
    /// <summary>
    /// Gets the entity's single value column.
    /// </summary>
    public string Value { get; private init; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoPkEntity"/> record.
    /// </summary>
    /// <param name="value">The value to store.</param>
    public NoPkEntity(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Private constructor for ORM usage.
    /// </summary>
    private NoPkEntity() { }
}
