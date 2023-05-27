using Annium.linq2db.Extensions.Models;
using Annium.linq2db.Extensions.Tests.Db.Models;
using Annium.Logging.Abstractions;
using LinqToDB;
using LinqToDB.Data;

namespace Annium.linq2db.Extensions.Tests.Db;

internal sealed class Connection : DataConnection, ILogSubject<Connection>
{
    public ILogger<Connection> Logger { get; }
    public ITable<Company> Companies { get; }
    public ITable<Employee> Employees { get; }
    public ITable<CompanyEmployee> CompanyEmployees { get; }

    public Connection(
        Config<Connection> config,
        ILogger<Connection> logger
    ) : base(config.Options)
    {
        Logger = logger;
        Companies = this.GetTable<Company>();
        Employees = this.GetTable<Employee>();
        CompanyEmployees = this.GetTable<CompanyEmployee>();
    }
}