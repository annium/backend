using System;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB;
using LinqToDB.Mapping;

namespace Annium.linq2db.Tests.Lib.Db.Configurations;

internal class CompanyConfiguration
    : IIdEntityConfiguration<Company, Guid>,
        ICreatedUpdatedTimeEntityConfiguration<Company>
{
    public void Configure(EntityMappingBuilder<Company> builder)
    {
        this.ConfigureId(builder);
        this.ConfigureAutoCreatedUpdatedTime(builder);
        builder.HasTableName("companies");
        builder.HasPrimaryKey(x => x.Id);
        builder.Property(x => x.Id).IsColumn();
        builder.Property(x => x.Name).IsColumn();
        builder.Property(x => x.Metadata).IsColumn().HasDbType("jsonb").HasDataType(DataType.Json);
        builder.Association(x => x.Employees, x => x.Id, x => x.CompanyId, false);
    }
}
