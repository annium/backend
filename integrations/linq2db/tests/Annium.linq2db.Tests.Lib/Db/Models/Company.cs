using System;
using System.Collections.Generic;
using Annium.Data.Models;
using NodaTime;

namespace Annium.linq2db.Tests.Lib.Db.Models;

/// <summary>
/// Represents a company entity with unique identifier, metadata, employees, and audit timestamps
/// </summary>
public sealed record Company : IIdEntity<Guid>, ICreatedUpdatedTimeEntity
{
    /// <summary>
    /// Gets the unique identifier for the company
    /// </summary>
    public Guid Id { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the company name
    /// </summary>
    public string Name { get; private init; } = string.Empty;

    /// <summary>
    /// Gets or sets the company metadata including address information
    /// </summary>
    public CompanyMetadata Metadata { get; private set; } = default!;

    /// <summary>
    /// Gets the collection of employees associated with this company
    /// </summary>
    public IReadOnlyCollection<CompanyEmployee> Employees { get; private init; } = Array.Empty<CompanyEmployee>();

    /// <summary>
    /// Gets the timestamp when the company was created
    /// </summary>
    public Instant CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the company was last updated
    /// </summary>
    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Company class
    /// </summary>
    /// <param name="name">Company name</param>
    /// <param name="metadata">Company metadata</param>
    public Company(string name, CompanyMetadata metadata)
    {
        Name = name;
        Metadata = metadata;
    }

    /// <summary>
    /// Private constructor for ORM usage
    /// </summary>
    private Company() { }

    /// <summary>
    /// Updates the company metadata
    /// </summary>
    /// <param name="metadata">New metadata to set</param>
    public void SetMetadata(CompanyMetadata metadata)
    {
        Metadata = metadata;
    }
}
