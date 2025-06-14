using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Tests.Lib.Db.Configurations;

/// <summary>
/// linq2db entity configuration for CompanyEmployee junction table with timestamp management
/// </summary>
internal class CompanyEmployeeConfiguration : ICreatedUpdatedTimeEntityConfiguration<CompanyEmployee>
{
    /// <summary>
    /// Configures the CompanyEmployee entity mapping including table name, composite primary key, and associations
    /// </summary>
    /// <param name="builder">Entity mapping builder for CompanyEmployee</param>
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
