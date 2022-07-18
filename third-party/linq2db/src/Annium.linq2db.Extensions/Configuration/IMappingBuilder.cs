using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration;

public interface IMappingBuilder
{
    FluentMappingBuilder Map { get; }
    IMappingBuilder ApplyConfigurations();
}