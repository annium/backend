using LinqToDB.Internal.SqlQuery;

namespace Annium.linq2db.Extensions.Internal.Extensions;

internal static class SqlStatementExtensions
{
    /// <summary>
    /// Creates a clone of the SQL statement, excluding SQL parameters.
    /// </summary>
    /// <param name="stmt">The original SQL statement.</param>
    /// <returns>A cloned SQL statement.</returns>
    public static T CloneWithoutParams<T>(this T stmt)
        where T : SqlStatement
    {
        var clone = stmt.Clone(e => e.ElementType != QueryElementType.SqlParameter);

        return clone;
    }
}
