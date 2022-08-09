using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Tests.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Tests.Db.Configurations;

internal class EmployeeConfiguration : IEntityConfiguration<Employee>
{
    public void Configure(EntityMappingBuilder<Employee> builder)
    {
        builder.HasTableName("employees");
        builder.HasPrimaryKey(x => x.Id);
        builder.Property(x => x.Id).IsColumn();
        builder.Association(x => x.Company, x => x.CompanyId, x => x.Id, false);
    }
}