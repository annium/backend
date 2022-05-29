using System;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions;

public interface IMappingBuilder
{
    FluentMappingBuilder Map { get; }
    IMappingBuilder ApplyConfigurations();
    IMappingBuilder Configure(Action<Database> configure, MetadataBuilderFlags flags = MetadataBuilderFlags.None);
}