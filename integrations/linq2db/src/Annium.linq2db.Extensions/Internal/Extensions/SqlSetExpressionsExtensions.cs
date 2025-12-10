using System.Collections.Generic;
using LinqToDB.Internal.SqlQuery;

namespace Annium.linq2db.Extensions.Internal.Extensions;

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
            if (expression.Column is SqlField f && f != null! && f.PhysicalName == field.PhysicalName)
                return expression;

        return null;
    }
}
