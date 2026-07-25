using Annium.linq2db.Extensions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Internal.SqlQuery;

namespace Annium.linq2db.Tests.Lib.Db;

/// <summary>
/// A second distinct DataConnection type used to exercise the multi-arity connection/transaction
/// scope overloads (which are generic over distinct connection types). Points at the same test
/// database as <see cref="Connection"/> but is a separate connection/transaction.
/// </summary>
public sealed class ConnectionB : DataConnection
{
    /// <summary>
    /// Time provider for timestamp management.
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionB"/> class.
    /// </summary>
    /// <param name="timeProvider">Time provider for timestamp operations.</param>
    /// <param name="config">Database connection configuration.</param>
    public ConnectionB(ITimeProvider timeProvider, DataOptions<ConnectionB> config)
        : base(config.Options)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Processes SQL queries to handle created/updated timestamp logic.
    /// </summary>
    /// <param name="statement">SQL statement to process.</param>
    /// <param name="context">Query evaluation context.</param>
    /// <returns>Processed SQL statement with timestamp handling.</returns>
    protected override SqlStatement ProcessQuery(SqlStatement statement, EvaluationContext context)
    {
        return this.ProcessCreatedUpdatedTimeQuery(statement, _timeProvider);
    }
}
