using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Tests.Lib.Db.Configurations;

/// <summary>
/// linq2db entity configuration for <see cref="NoPkEntity"/> — a table mapping with no primary key,
/// used to exercise the empty-primary-key edge of the update predicate builder.
/// </summary>
internal class NoPkEntityConfiguration : IEntityConfiguration<NoPkEntity>
{
    /// <summary>
    /// Configures the NoPkEntity mapping with a table name and a single column, deliberately without a primary key.
    /// </summary>
    /// <param name="builder">Entity mapping builder for NoPkEntity.</param>
    public void Configure(EntityMappingBuilder<NoPkEntity> builder)
    {
        builder.HasTableName("no_pk_entities");
        builder.Property(x => x.Value).IsColumn();
    }
}
