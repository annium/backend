using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions;

public class DataConnectionBase : DataConnection
{
    public DataConnectionBase(
        string providerName,
        string connectionString,
        MappingSchema mappingSchema
    ) : base(providerName, connectionString)
    {
        AddMappingSchema(mappingSchema);
    }
}