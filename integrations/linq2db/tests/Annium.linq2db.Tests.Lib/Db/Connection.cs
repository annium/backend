using Annium.linq2db.Tests.Lib.Db.Models;
using Annium.Logging;
using LinqToDB;
using LinqToDB.Data;

namespace Annium.linq2db.Tests.Lib.Db;

public sealed class Connection : DataConnection
{
    public ITable<Company> Companies { get; }
    public ITable<Employee> Employees { get; }
    public ITable<CompanyEmployee> CompanyEmployees { get; }

    public Connection(DataOptions<Connection> config)
        : base(config.Options)
    {
        Companies = this.GetTable<Company>();
        Employees = this.GetTable<Employee>();
        CompanyEmployees = this.GetTable<CompanyEmployee>();
    }
}
