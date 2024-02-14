using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Tests.Lib.Db.Configurations;

internal class CompanyEmployeeConfiguration : ICreatedUpdatedTimeEntityConfiguration<CompanyEmployee>
{
    public void Configure(EntityMappingBuilder<CompanyEmployee> builder)
    {
        this.ConfigureAutoCreatedUpdatedTime(builder);
        builder.HasTableName("company_employees");
        builder.HasPrimaryKey(x => new { x.CompanyId, x.EmployeeId });
        builder.Association(x => x.Company, x => x.CompanyId, x => x.Id, false);
        builder.Association(x => x.Employee, x => x.EmployeeId, x => x.Id, false);
        builder.Property(x => x.Role).IsColumn();
    }
}
