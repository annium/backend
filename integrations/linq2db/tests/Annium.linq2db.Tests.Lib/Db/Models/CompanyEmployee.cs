using System;

namespace Annium.linq2db.Tests.Lib.Db.Models;

public sealed record CompanyEmployee
{
    public Guid CompanyId { get; private init; }
    public Company Company { get; private init; } = default!;
    public Guid EmployeeId { get; private init; }
    public Employee Employee { get; private init; } = default!;
    public string Role { get; private set; } = string.Empty;

    public CompanyEmployee(Company company, Employee employee, string role)
    {
        CompanyId = company.Id;
        Company = company;
        EmployeeId = employee.Id;
        Employee = employee;
        Role = role;
    }

    private CompanyEmployee() { }

    public void SetRole(string role) => Role = role;
}
