using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Tests.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Tests.Db.Configurations;

internal class CompanyEmployeeConfiguration : IEntityConfiguration<CompanyEmployee>
{
    public void Configure(EntityMappingBuilder<CompanyEmployee> builder)
    {
        builder.HasTableName("company_employees");
        builder.HasPrimaryKey(x => new { x.CompanyId, x.EmployeeId });
        builder.Association(x => x.Company, x => x.CompanyId, x => x.Id, false);
        builder.Association(x => x.Employee, x => x.EmployeeId, x => x.Id, false);
        builder.Property(x => x.Role).IsColumn();
    }
}