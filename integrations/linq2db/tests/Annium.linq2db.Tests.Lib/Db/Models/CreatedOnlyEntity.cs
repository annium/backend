using System;
using Annium.Data.Models;
using NodaTime;

namespace Annium.linq2db.Tests.Lib.Db.Models;

/// <summary>
/// An entity that tracks a created timestamp but no updated timestamp (implements
/// <see cref="ICreatedTimeEntity"/> but not <see cref="ICreatedUpdatedTimeEntity"/>), used to
/// exercise the created-only branch of the auto-timestamp query processing and the public
/// <c>ConfigureAutoCreatedTime</c> configuration API.
/// </summary>
public sealed record CreatedOnlyEntity : IIdEntity<Guid>, ICreatedTimeEntity
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the entity's content.
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when the entity was created.
    /// </summary>
    public Instant CreatedAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatedOnlyEntity"/> record.
    /// </summary>
    /// <param name="content">The content to store.</param>
    public CreatedOnlyEntity(string content)
    {
        Content = content;
    }

    /// <summary>
    /// Private constructor for ORM usage.
    /// </summary>
    private CreatedOnlyEntity() { }

    /// <summary>
    /// Updates the entity's content.
    /// </summary>
    /// <param name="content">New content to set.</param>
    public void SetContent(string content) => Content = content;
}
