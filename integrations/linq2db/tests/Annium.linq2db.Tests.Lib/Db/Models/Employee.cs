using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Data.Models;
using NodaTime;

namespace Annium.linq2db.Tests.Lib.Db.Models;

/// <summary>
/// Represents an employee entity with hierarchical relationships, unique identifier, and audit timestamps
/// </summary>
public sealed record Employee : IIdEntity<Guid>, ICreatedUpdatedTimeEntity
{
    /// <summary>
    /// Gets the unique identifier for the employee
    /// </summary>
    public Guid Id { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the employee name
    /// </summary>
    public string Name { get; private init; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the employee's chief (manager)
    /// </summary>
    public Guid? ChiefId { get; private set; }

    /// <summary>
    /// Gets or sets the employee's chief (manager)
    /// </summary>
    public Employee? Chief { get; private set; }

    /// <summary>
    /// Gets the timestamp when the employee was created
    /// </summary>
    public Instant CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the employee was last updated
    /// </summary>
    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Gets the collection of employees who report to this employee
    /// </summary>
    public IReadOnlyCollection<Employee> Subordinates
    {
        get => _subordinates;
        private init => _subordinates = value.ToList();
    }

    /// <summary>
    /// Internal list of subordinate employees
    /// </summary>
    private readonly List<Employee> _subordinates = new();

    /// <summary>
    /// Initializes a new instance of the Employee class
    /// </summary>
    /// <param name="name">Employee name</param>
    /// <param name="chief">Employee's manager/chief</param>
    public Employee(string name, Employee? chief)
    {
        Id = Guid.NewGuid();
        Name = name;
        ChiefId = chief?.Id;
        Chief = chief;
    }

    /// <summary>
    /// Private constructor for ORM usage
    /// </summary>
    private Employee() { }

    /// <summary>
    /// Updates the employee's chief (manager)
    /// </summary>
    /// <param name="chief">New chief to set, or null to remove current chief</param>
    public void SetChief(Employee? chief)
    {
        ChiefId = chief?.Id;
        Chief = chief;
    }
}
