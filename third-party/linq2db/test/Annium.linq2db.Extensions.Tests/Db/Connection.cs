using Annium.linq2db.Extensions.Models;
using Annium.linq2db.Extensions.Tests.Db.Models;
using LinqToDB;
using LinqToDB.Configuration;

namespace Annium.linq2db.Extensions.Tests.Db;

internal sealed class Connection : DataConnectionBase
{
    public ITable<Company> Companies { get; }
    public ITable<Employee> Employees { get; }

    public Connection(LinqToDBConnectionOptions options) : base(options)
    {
        Companies = this.GetTable<Company>();
        Employees = this.GetTable<Employee>();
    }
}