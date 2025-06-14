using System;
using System.Collections.Generic;
using Annium.Data.Models;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.SqlQuery;
using NodaTime;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Extension methods for DataConnection to handle automatic created/updated time management in SQL queries.
/// </summary>
public static class DataConnectionExtensions
{
    /// <summary>
    /// Processes SQL statements to automatically inject created/updated time values based on query type.
    /// </summary>
    /// <param name="dc">The data connection.</param>
    /// <param name="statement">The SQL statement to process.</param>
    /// <param name="timeProvider">The time provider for current timestamp.</param>
    /// <returns>The processed SQL statement with time fields added where appropriate.</returns>
    public static SqlStatement ProcessCreatedUpdatedTimeQuery(
        this DataConnection dc,
        SqlStatement statement,
        ITimeProvider timeProvider
    )
    {
        return statement.QueryType switch
        {
            QueryType.Insert => dc.ProcessInsertTimeQuery(statement, timeProvider),
            QueryType.Update => dc.ProcessUpdateTimeQuery(statement, timeProvider),
            QueryType.InsertOrUpdate => dc.ProcessInsertOrUpdateTimeQuery(statement, timeProvider),
            _ => statement,
        };
    }

    /// <summary>
    /// Processes INSERT queries to add created and updated time values.
    /// </summary>
    /// <param name="cn">The data connection.</param>
    /// <param name="statement">The SQL INSERT statement.</param>
    /// <param name="timeProvider">The time provider for current timestamp.</param>
    /// <returns>The processed SQL statement with time fields added.</returns>
    private static SqlStatement ProcessInsertTimeQuery(
        this DataConnection cn,
        SqlStatement statement,
        ITimeProvider timeProvider
    )
    {
        var source = statement.RequireInsertClause().Into.NotNull();
        var descriptor = cn.MappingSchema.GetEntityDescriptor(source.ObjectType);

        var createdTime = source.ObjectType.IsAssignableTo(typeof(ICreatedTimeEntity))
            ? descriptor.Columns.FindColumn(nameof(ICreatedTimeEntity.CreatedAt))
            : null;
        var updatedTime = source.ObjectType.IsAssignableTo(typeof(ICreatedUpdatedTimeEntity))
            ? descriptor.Columns.FindColumn(nameof(ICreatedUpdatedTimeEntity.UpdatedAt))
            : null;

        if (createdTime is null && updatedTime is null)
            return statement;

        var stmt = Clone(statement);
        var insert = stmt.RequireInsertClause();
        var insertTable = insert.Into.NotNull();
        var now = timeProvider.Now;

        if (createdTime is not null)
            AddSetColumnTimeExpression(insertTable, insert.Items, createdTime, now);

        if (updatedTime is not null)
            AddSetColumnTimeExpression(insertTable, insert.Items, updatedTime, now);

        return stmt;
    }

    /// <summary>
    /// Processes UPDATE queries to add updated time values.
    /// </summary>
    /// <param name="cn">The data connection.</param>
    /// <param name="statement">The SQL UPDATE statement.</param>
    /// <param name="timeProvider">The time provider for current timestamp.</param>
    /// <returns>The processed SQL statement with updated time field added.</returns>
    private static SqlStatement ProcessUpdateTimeQuery(
        this DataConnection cn,
        SqlStatement statement,
        ITimeProvider timeProvider
    )
    {
        var updateTable = GetUpdateTable(statement);
        if (updateTable is null)
            return statement;

        var descriptor = cn.MappingSchema.GetEntityDescriptor(updateTable.ObjectType);

        var updatedTime = updateTable.ObjectType.IsAssignableTo(typeof(ICreatedUpdatedTimeEntity))
            ? descriptor.Columns.FindColumn(nameof(ICreatedUpdatedTimeEntity.UpdatedAt))
            : null;

        if (updatedTime is null)
            return statement;

        var stmt = Clone(statement);

        updateTable = GetUpdateTable(stmt) ?? throw new InvalidOperationException();
        var update = stmt.RequireUpdateClause().NotNull();

        AddSetColumnTimeExpression(updateTable, update.Items, updatedTime, timeProvider.Now);

        return stmt;
    }

    /// <summary>
    /// Processes INSERT OR UPDATE (UPSERT) queries to manage both created and updated time values appropriately.
    /// </summary>
    /// <param name="cn">The data connection.</param>
    /// <param name="statement">The SQL INSERT OR UPDATE statement.</param>
    /// <param name="timeProvider">The time provider for current timestamp.</param>
    /// <returns>The processed SQL statement with time fields managed for both insert and update operations.</returns>
    private static SqlStatement ProcessInsertOrUpdateTimeQuery(
        this DataConnection cn,
        SqlStatement statement,
        ITimeProvider timeProvider
    )
    {
        var source = statement.RequireInsertClause().Into.NotNull();
        var updateTable = GetUpdateTable(statement);
        if (updateTable is null)
            return statement;

        var descriptor = cn.MappingSchema.GetEntityDescriptor(source.ObjectType);

        var createdTime = source.ObjectType.IsAssignableTo(typeof(ICreatedTimeEntity))
            ? descriptor.Columns.FindColumn(nameof(ICreatedTimeEntity.CreatedAt))
            : null;
        var updatedTime = source.ObjectType.IsAssignableTo(typeof(ICreatedUpdatedTimeEntity))
            ? descriptor.Columns.FindColumn(nameof(ICreatedUpdatedTimeEntity.UpdatedAt))
            : null;

        if (createdTime is null && updatedTime is null)
            return statement;

        var stmt = Clone(statement);
        var insert = stmt.RequireInsertClause();
        var insertTable = insert.Into.NotNull();
        updateTable = GetUpdateTable(stmt) ?? throw new InvalidOperationException();
        var update = stmt.RequireUpdateClause().NotNull();
        var now = timeProvider.Now;

        if (createdTime is not null)
        {
            AddSetColumnTimeExpression(insertTable, insert.Items, createdTime, now);
            DeleteSetColumnTimeExpression(updateTable, update.Items, createdTime);
        }

        if (updatedTime is not null)
        {
            AddSetColumnTimeExpression(insertTable, insert.Items, updatedTime, now);
            AddSetColumnTimeExpression(updateTable, update.Items, updatedTime, now);
        }

        return stmt;
    }

    /// <summary>
    /// Adds or updates a time column expression in the SQL statement.
    /// </summary>
    /// <param name="table">The SQL table.</param>
    /// <param name="expressions">The list of set expressions.</param>
    /// <param name="desc">The column descriptor.</param>
    /// <param name="now">The current time instant.</param>
    private static void AddSetColumnTimeExpression(
        SqlTable table,
        List<SqlSetExpression> expressions,
        ColumnDescriptor desc,
        Instant now
    )
    {
        var field = table.FindFieldByMemberName(desc.MemberName).NotNull();
        var column = expressions.FindField(field);
        if (column is null)
            expressions.Add(new SqlSetExpression(field, new SqlValue(typeof(Instant), now)));
        else
            column.Expression = new SqlValue(typeof(Instant), now);
    }

    /// <summary>
    /// Removes a time column expression from the SQL statement.
    /// </summary>
    /// <param name="table">The SQL table.</param>
    /// <param name="expressions">The list of set expressions.</param>
    /// <param name="desc">The column descriptor.</param>
    private static void DeleteSetColumnTimeExpression(
        SqlTable table,
        List<SqlSetExpression> expressions,
        ColumnDescriptor desc
    )
    {
        var field = table.FindFieldByMemberName(desc.MemberName).NotNull();
        var column = expressions.FindField(field);
        expressions.RemoveAll(x => x == column);
    }

    /// <summary>
    /// Finds a column descriptor by name that doesn't have skip values configured.
    /// </summary>
    /// <param name="columns">The collection of column descriptors.</param>
    /// <param name="name">The column name to find.</param>
    /// <returns>The matching column descriptor or null if not found.</returns>
    private static ColumnDescriptor? FindColumn(this IEnumerable<ColumnDescriptor> columns, string name)
    {
        foreach (var column in columns)
            if (
                column.MemberAccessor.MemberInfo.Name == name
                && column is { HasValuesToSkipOnInsert: false, HasValuesToSkipOnUpdate: false }
            )
                return column;

        return null;
    }

    /// <summary>
    /// Finds a set expression for a specific SQL field.
    /// </summary>
    /// <param name="expressions">The collection of set expressions.</param>
    /// <param name="field">The SQL field to find.</param>
    /// <returns>The matching set expression or null if not found.</returns>
    private static SqlSetExpression? FindField(this IEnumerable<SqlSetExpression> expressions, SqlField field)
    {
        foreach (var expression in expressions)
            if (expression.Column is SqlField f && f != null! && f.PhysicalName == field.PhysicalName)
                return expression;

        return null;
    }

    /// <summary>
    /// Creates a clone of the SQL statement, excluding SQL parameters.
    /// </summary>
    /// <param name="original">The original SQL statement.</param>
    /// <returns>A cloned SQL statement.</returns>
    private static SqlStatement Clone(SqlStatement original)
    {
        var clone = original.Clone(e => e.ElementType != QueryElementType.SqlParameter);

        return clone;
    }

    /// <summary>
    /// Gets the update table from a SQL statement.
    /// </summary>
    /// <param name="statement">The SQL statement.</param>
    /// <returns>The update table or null if not found.</returns>
    private static SqlTable? GetUpdateTable(SqlStatement statement)
    {
        if (statement is SqlUpdateStatement update)
            return update.GetUpdateTable();

        if (statement.SelectQuery == null)
            return null;

        if (
            statement.SelectQuery.From.Tables.Count > 0
            && statement.SelectQuery?.From.Tables[0].Source is SqlTable source
        )
        {
            return source;
        }

        return null;
    }
}
