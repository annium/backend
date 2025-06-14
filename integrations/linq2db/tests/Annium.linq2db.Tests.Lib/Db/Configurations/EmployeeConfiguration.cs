using System;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Tests.Lib.Db.Configurations;

/// <summary>
/// linq2db entity configuration for Employee entities with hierarchical relationships, ID and timestamp management
/// </summary>
internal class EmployeeConfiguration
    : IIdEntityConfiguration<Employee, Guid>,
        ICreatedUpdatedTimeEntityConfiguration<Employee>
{
    /// <summary>
    /// Configures the Employee entity mapping including table name, primary key, columns, and self-referential associations
    /// </summary>
    /// <param name="builder">Entity mapping builder for Employee</param>
    public void Configure(EntityMappingBuilder<Employee> builder)
    {
        this.ConfigureId(builder);
        this.ConfigureAutoCreatedUpdatedTime(builder);
        builder.HasTableName("employees");
        builder.HasPrimaryKey(x => x.Id);
        builder.Property(x => x.Id).IsColumn();
        builder.Property(x => x.Name).IsColumn();
        builder.Association(x => x.Chief, x => x.ChiefId, x => x!.Id);
        builder.Association(x => x.Subordinates, x => x.Id, x => x.ChiefId);
    }
}
