using LinqToDB.Internal.SqlQuery;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Extensions;

/// <summary>
/// Extension methods for manipulating linq2db <see cref="LinqToDB.Internal.SqlQuery.SqlUpdateClause"/> items — setting or ignoring column values by column descriptor.
/// </summary>
internal static class SqlUpdateClauseExtensions
{
    /// <summary>
    /// Updates clause to set specified column to given value.
    /// </summary>
    /// <param name="clause">The SQL update clause.</param>
    /// <param name="desc">The column descriptor.</param>
    /// <param name="value">The value to set the column to.</param>
    public static void SetValue<T>(this SqlUpdateClause clause, ColumnDescriptor desc, T value)
    {
        var field = clause.FindField(desc);
        var column = clause.Items.FindField(field);

        if (column is null)
            clause.Items.Add(new SqlSetExpression(field, new SqlValue(typeof(T), value)));
        else
            column.Expression = new SqlValue(typeof(T), value);
    }

    /// <summary>
    /// Updates clause to ignore specified column.
    /// </summary>
    /// <param name="clause">The SQL update clause.</param>
    /// <param name="desc">The column descriptor.</param>
    public static void IgnoreValue(this SqlUpdateClause clause, ColumnDescriptor desc)
    {
        var field = clause.FindField(desc);
        var column = clause.Items.FindField(field);

        if (column is not null)
            clause.Items.Remove(column);
    }

    /// <summary>
    /// Resolves the clause's field for the given column descriptor.
    /// </summary>
    /// <param name="clause">The SQL update clause.</param>
    /// <param name="desc">The column descriptor.</param>
    /// <returns>The field backing the described column.</returns>
    /// <remarks>
    /// linq2db 6.4 widened <see cref="SqlUpdateClause.Table"/> from <c>SqlTable</c> to
    /// <see cref="ISqlNamedTable"/>, but only the concrete table exposes the mapped fields. Both
    /// callers run behind an entity-type resolution that already established a mapped table, so a
    /// cast is the right assertion here.
    /// </remarks>
    private static SqlField FindField(this SqlUpdateClause clause, ColumnDescriptor desc) =>
        clause.Table.NotNull().CastTo<SqlTable>().FindFieldByMemberName(desc.MemberName).NotNull();
}
