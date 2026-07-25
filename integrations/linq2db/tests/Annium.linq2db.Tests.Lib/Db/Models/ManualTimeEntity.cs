using System;
using Annium.Data.Models;
using NodaTime;

namespace Annium.linq2db.Tests.Lib.Db.Models;

/// <summary>
/// An entity whose created/updated timestamps are managed by the application (configured via
/// ConfigureManualCreatedUpdatedTime), used to exercise the manual-timestamp path through a
/// Connection whose ProcessQuery routes into the auto-timestamp pipeline. The auto pipeline must
/// leave the manually-managed columns untouched.
/// </summary>
public sealed record ManualTimeEntity : IIdEntity<Guid>, ICreatedUpdatedTimeEntity
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
    /// Gets the application-managed created timestamp.
    /// </summary>
    public Instant CreatedAt { get; private set; }

    /// <summary>
    /// Gets the application-managed updated timestamp.
    /// </summary>
    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualTimeEntity"/> record.
    /// </summary>
    /// <param name="content">The content to store.</param>
    public ManualTimeEntity(string content)
    {
        Content = content;
    }

    /// <summary>
    /// Private constructor for ORM usage.
    /// </summary>
    private ManualTimeEntity() { }

    /// <summary>
    /// Updates the entity's content.
    /// </summary>
    /// <param name="content">New content to set.</param>
    public void SetContent(string content) => Content = content;

    /// <summary>
    /// Sets the application-managed created timestamp.
    /// </summary>
    /// <param name="createdAt">The created timestamp.</param>
    public void SetCreatedAt(Instant createdAt) => CreatedAt = createdAt;

    /// <summary>
    /// Sets the application-managed updated timestamp.
    /// </summary>
    /// <param name="updatedAt">The updated timestamp.</param>
    public void SetUpdatedAt(Instant updatedAt) => UpdatedAt = updatedAt;
}
