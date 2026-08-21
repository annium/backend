using LinqToDB.Internal.SqlQuery;

namespace Annium.linq2db.Extensions.Internal.Extensions;

/// <summary>
/// Extension methods for linq2db <see cref="LinqToDB.Internal.SqlQuery.SqlStatement"/> that produce clones with specific query elements removed.
/// </summary>
internal static class SqlStatementExtensions
{
    /// <summary>
    /// Creates a clone of the SQL statement, excluding SQL parameters.
    /// </summary>
    /// <param name="stmt">The original SQL statement.</param>
    /// <returns>A cloned SQL statement.</returns>
    /// <typeparam name="T">Concrete statement type, preserved by the clone.</typeparam>
    public static T CloneWithoutParams<T>(this T stmt)
        where T : SqlStatement
    {
        var clone = stmt.Clone(e => e.ElementType != QueryElementType.SqlParameter);

        return clone;
    }
}
