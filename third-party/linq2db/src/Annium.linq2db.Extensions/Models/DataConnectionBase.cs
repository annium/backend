using LinqToDB.Configuration;
using LinqToDB.Data;

namespace Annium.linq2db.Extensions.Models;

public class DataConnectionBase : DataConnection
{
    public DataConnectionBase(
        LinqToDBConnectionOptions options
    ) : base(options)
    {
        if (options.MappingSchema is not null)
            AddMappingSchema(options.MappingSchema);
    }
}