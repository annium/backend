using System;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Tests.Lib.Db.Configurations;

/// <summary>
/// linq2db entity configuration for <see cref="ManualTimeEntity"/> — an ID entity whose created/updated
/// timestamps are application-managed (configured via <c>ConfigureManualCreatedUpdatedTime</c>, which marks
/// the timestamp columns to be skipped by the auto-timestamp pipeline).
/// </summary>
internal class ManualTimeEntityConfiguration
    : IIdEntityConfiguration<ManualTimeEntity, Guid>,
        ICreatedUpdatedTimeEntityConfiguration<ManualTimeEntity>
{
    /// <summary>
    /// Configures the ManualTimeEntity mapping including table name, primary key, content column, and manual timestamps.
    /// </summary>
    /// <param name="builder">Entity mapping builder for ManualTimeEntity.</param>
    public void Configure(EntityMappingBuilder<ManualTimeEntity> builder)
    {
        this.ConfigureId(builder);
        this.ConfigureManualCreatedUpdatedTime(builder);
        builder.HasTableName("manual_time_entities");
        builder.Property(x => x.Content).IsColumn();
    }
}
