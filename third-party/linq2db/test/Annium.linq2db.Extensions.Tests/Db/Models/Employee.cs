using System;

namespace Annium.linq2db.Extensions.Tests.Db.Models;

internal sealed record Employee
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public Guid CompanyId { get; private init; }
    public Company Company { get; private init; }
    public string Name { get; private init; } = string.Empty;

    public Employee(Company company, string name)
    {
        CompanyId = company.Id;
        Company = company;
        Name = name;
    }
}