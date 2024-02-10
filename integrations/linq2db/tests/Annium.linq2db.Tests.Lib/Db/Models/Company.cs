using System;
using System.Collections.Generic;
using Annium.Data.Models;
using NodaTime;

namespace Annium.linq2db.Tests.Lib.Db.Models;

public sealed record Company : IIdEntity<Guid>, ICreatedUpdatedTimeEntity
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Name { get; private init; } = string.Empty;
    public CompanyMetadata Metadata { get; private set; } = default!;
    public IReadOnlyCollection<CompanyEmployee> Employees { get; private init; } = Array.Empty<CompanyEmployee>();
    public Instant CreatedAt { get; private set; }
    public Instant UpdatedAt { get; private set; }

    public Company(string name, CompanyMetadata metadata)
    {
        Name = name;
        Metadata = metadata;
    }

    private Company() { }

    public void SetMetadata(CompanyMetadata metadata)
    {
        Metadata = metadata;
    }
}
