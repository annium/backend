using System;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Tests.Lib.Db.Configurations;

internal class EmployeeConfiguration
    : IIdEntityConfiguration<Employee, Guid>,
        ICreatedUpdatedTimeEntityConfiguration<Employee>
{
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
