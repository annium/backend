using LinqToDB.Internal.SqlQuery;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Extensions;

internal static class SqlInsertClauseExtensions
{
    /// <summary>
    /// Updates clause to set specified column to given value.
    /// </summary>
    /// <param name="clause">The SQL insert clause.</param>
    /// <param name="desc">The column descriptor.</param>
    /// <param name="value">The current time instant.</param>
    public static void SetValue<T>(this SqlInsertClause clause, ColumnDescriptor desc, T value)
    {
        var field = clause.Into.NotNull().FindFieldByMemberName(desc.MemberName).NotNull();
        var column = clause.Items.FindField(field);

        if (column is null)
            clause.Items.Add(new SqlSetExpression(field, new SqlValue(typeof(T), value)));
        else
            column.Expression = new SqlValue(typeof(T), value);
    }

    /// <summary>
    /// Updates clause to ignore specified column.
    /// </summary>
    /// <param name="clause">The SQL insert clause.</param>
    /// <param name="desc">The column descriptor.</param>
    public static void IgnoreValue(this SqlInsertClause clause, ColumnDescriptor desc)
    {
        var field = clause.Into.NotNull().FindFieldByMemberName(desc.MemberName).NotNull();
        var column = clause.Items.FindField(field);

        if (column is not null)
            clause.Items.Remove(column);
    }
}
