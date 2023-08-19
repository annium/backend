using System;
using System.Collections.Generic;
using NodaTime;

namespace Annium.linq2db.Tests.Lib.Db.Models;

public sealed record Company
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Name { get; private init; } = string.Empty;
    public Instant CreatedAt { get; private set; }
    public CompanyMetadata Metadata { get; private set; } = default!;
    public IReadOnlyCollection<CompanyEmployee> Employees { get; private init; } = Array.Empty<CompanyEmployee>();

    public Company(string name, Instant createdAt, CompanyMetadata metadata)
    {
        Name = name;
        CreatedAt = createdAt;
        Metadata = metadata;
    }

    private Company()
    {
    }

    public void SetCreatedAt(Instant createdAt)
    {
        CreatedAt = createdAt;
    }

    public void SetMetadata(CompanyMetadata metadata)
    {
        Metadata = metadata;
    }
}