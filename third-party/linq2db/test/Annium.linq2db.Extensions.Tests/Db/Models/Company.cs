using System;
using System.Collections.Generic;

namespace Annium.linq2db.Extensions.Tests.Db.Models;

internal sealed record Company
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public CompanyMetadata Metadata { get; init; } = default!;
    public IReadOnlyCollection<Employee> Employees { get; private init; } = default!;

    public Company(string name, CompanyMetadata metadata)
    {
        Name = name;
        Metadata = metadata;
    }

    private Company()
    {
    }
}