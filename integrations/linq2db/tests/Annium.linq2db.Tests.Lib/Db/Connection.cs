using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.SqlQuery;

namespace Annium.linq2db.Tests.Lib.Db;

public sealed class Connection : DataConnection
{
    public ITable<Company> Companies => this.GetTable<Company>();
    public ITable<Employee> Employees => this.GetTable<Employee>();
    public ITable<CompanyEmployee> CompanyEmployees => this.GetTable<CompanyEmployee>();
    private readonly ITimeProvider _timeProvider;

    public Connection(ITimeProvider timeProvider, DataOptions<Connection> config)
        : base(config.Options)
    {
        _timeProvider = timeProvider;
    }

    protected override SqlStatement ProcessQuery(SqlStatement statement, EvaluationContext context)
    {
        return this.ProcessCreatedUpdatedTimeQuery(statement, _timeProvider);
    }
}