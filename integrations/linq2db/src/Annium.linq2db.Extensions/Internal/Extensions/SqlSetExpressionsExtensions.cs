using System.Collections.Generic;
using LinqToDB.Internal.SqlQuery;

namespace Annium.linq2db.Extensions.Internal.Extensions;

/// <summary>
/// Extension methods for searching a collection of linq2db <see cref="LinqToDB.Internal.SqlQuery.SqlSetExpression"/> items by target SQL field.
/// </summary>
internal static class SqlSetExpressionsExtensions
{
    /// <summary>
    /// Finds a set expression for a specific SQL field.
    /// </summary>
    /// <param name="expressions">The collection of set expressions.</param>
    /// <param name="field">The SQL field to find.</param>
    /// <returns>The matching set expression or null if not found.</returns>
    public static SqlSetExpression? FindField(this IEnumerable<SqlSetExpression> expressions, SqlField field)
    {
        foreach (var expression in expressions)
            if (expression.Column is SqlField f && f.PhysicalName == field.PhysicalName)
                return expression;

        return null;
    }
}
