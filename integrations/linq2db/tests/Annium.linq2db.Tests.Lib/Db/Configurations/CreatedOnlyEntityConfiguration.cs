using System;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Tests.Lib.Db.Configurations;

/// <summary>
/// linq2db entity configuration for <see cref="CreatedOnlyEntity"/> — an ID entity with auto-managed
/// created-only timestamp tracking (no updated timestamp), configured via <c>ConfigureAutoCreatedTime</c>.
/// </summary>
internal class CreatedOnlyEntityConfiguration
    : IIdEntityConfiguration<CreatedOnlyEntity, Guid>,
        ICreatedTimeEntityConfiguration<CreatedOnlyEntity>
{
    /// <summary>
    /// Configures the CreatedOnlyEntity mapping including table name, primary key, content column, and created-only timestamp.
    /// </summary>
    /// <param name="builder">Entity mapping builder for CreatedOnlyEntity.</param>
    public void Configure(EntityMappingBuilder<CreatedOnlyEntity> builder)
    {
        this.ConfigureId(builder);
        this.ConfigureAutoCreatedTime(builder);
        builder.HasTableName("created_only_entities");
        builder.Property(x => x.Content).IsColumn();
    }
}
