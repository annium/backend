using Annium.linq2db.Tests.Lib.Db.Models;
using Annium.Logging.Abstractions;
using LinqToDB;
using LinqToDB.Data;

namespace Annium.linq2db.Tests.Lib.Db;

public sealed class Connection : DataConnection, ILogSubject
{
    public ILogger Logger { get; }
    public ITable<Company> Companies { get; }
    public ITable<Employee> Employees { get; }
    public ITable<CompanyEmployee> CompanyEmployees { get; }

    public Connection(
        DataOptions<Connection> config,
        ILogger logger
    ) : base(config.Options)
    {
        Logger = logger;
        Companies = this.GetTable<Company>();
        Employees = this.GetTable<Employee>();
        CompanyEmployees = this.GetTable<CompanyEmployee>();
    }
}