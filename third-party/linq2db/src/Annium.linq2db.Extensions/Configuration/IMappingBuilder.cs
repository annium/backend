using System;
using Annium.linq2db.Extensions.Configuration.Schema;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration;

public interface IMappingBuilder
{
    MappingSchema Schema { get; }
    FluentMappingBuilder Map { get; }
    IMappingBuilder ApplyConfigurations();
    Database BuildMetadata(MetadataBuilderFlags flags = MetadataBuilderFlags.None);
    IMappingBuilder Configure(Action<Database> configure, MetadataBuilderFlags flags = MetadataBuilderFlags.None);
}