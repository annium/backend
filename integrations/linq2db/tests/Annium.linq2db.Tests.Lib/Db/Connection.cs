using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db.Models;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Internal.SqlQuery;

namespace Annium.linq2db.Tests.Lib.Db;

/// <summary>
/// Database connection class for linq2db test library with custom query processing for timestamps
/// </summary>
public sealed class Connection : DataConnection
{
    /// <summary>
    /// Gets the Companies table for database operations
    /// </summary>
    public ITable<Company> Companies => this.GetTable<Company>();

    /// <summary>
    /// Gets the Employees table for database operations
    /// </summary>
    public ITable<Employee> Employees => this.GetTable<Employee>();

    /// <summary>
    /// Gets the CompanyEmployees junction table for database operations
    /// </summary>
    public ITable<CompanyEmployee> CompanyEmployees => this.GetTable<CompanyEmployee>();

    /// <summary>
    /// Time provider for timestamp management
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the Connection class
    /// </summary>
    /// <param name="timeProvider">Time provider for timestamp operations</param>
    /// <param name="config">Database connection configuration</param>
    public Connection(ITimeProvider timeProvider, DataOptions<Connection> config)
        : base(config.Options)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Processes SQL queries to handle created/updated timestamp logic
    /// </summary>
    /// <param name="statement">SQL statement to process</param>
    /// <param name="context">Query evaluation context</param>
    /// <returns>Processed SQL statement with timestamp handling</returns>
    protected override SqlStatement ProcessQuery(SqlStatement statement, EvaluationContext context)
    {
        return this.ProcessCreatedUpdatedTimeQuery(statement, _timeProvider);
    }
}
