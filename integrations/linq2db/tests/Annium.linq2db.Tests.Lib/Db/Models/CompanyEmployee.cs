using System;
using Annium.Data.Models;
using NodaTime;

namespace Annium.linq2db.Tests.Lib.Db.Models;

/// <summary>
/// Represents a junction entity linking companies and employees with role information and audit timestamps
/// </summary>
public sealed record CompanyEmployee : ICreatedUpdatedTimeEntity
{
    /// <summary>
    /// Gets the company identifier
    /// </summary>
    public Guid CompanyId { get; private init; }

    /// <summary>
    /// Gets the company navigation property
    /// </summary>
    public Company Company { get; private init; } = default!;

    /// <summary>
    /// Gets the employee identifier
    /// </summary>
    public Guid EmployeeId { get; private init; }

    /// <summary>
    /// Gets the employee navigation property
    /// </summary>
    public Employee Employee { get; private init; } = default!;

    /// <summary>
    /// Gets or sets the employee's role in the company
    /// </summary>
    public string Role { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when the relationship was created
    /// </summary>
    public Instant CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the relationship was last updated
    /// </summary>
    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the CompanyEmployee class
    /// </summary>
    /// <param name="company">Company entity</param>
    /// <param name="employee">Employee entity</param>
    /// <param name="role">Employee's role in the company</param>
    public CompanyEmployee(Company company, Employee employee, string role)
    {
        CompanyId = company.Id;
        Company = company;
        EmployeeId = employee.Id;
        Employee = employee;
        Role = role;
    }

    /// <summary>
    /// Private constructor for ORM usage
    /// </summary>
    private CompanyEmployee() { }

    /// <summary>
    /// Updates the employee's role in the company
    /// </summary>
    /// <param name="role">New role to set</param>
    public void SetRole(string role) => Role = role;
}
