using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.linq2db.Tests.Lib.Db.Models;

public sealed record Employee
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Name { get; private init; } = string.Empty;
    public Guid? ChiefId { get; private set; }
    public Employee? Chief { get; private set; }

    public IReadOnlyCollection<Employee> Subordinates
    {
        get => _subordinates;
        private init => _subordinates = value.ToList();
    }

    private readonly List<Employee> _subordinates = new();

    public Employee(string name, Employee? chief)
    {
        Id = Guid.NewGuid();
        Name = name;
        ChiefId = chief?.Id;
        Chief = chief;
    }

    private Employee()
    {
    }

    public void SetChief(Employee? chief)
    {
        ChiefId = chief?.Id;
        Chief = chief;
    }
}