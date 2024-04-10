using System;
using System.Collections.Generic;
using Annium.Data.Models;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.SqlQuery;
using NodaTime;

namespace Annium.linq2db.Extensions.Configuration;

public static class DataConnectionExtensions
{
    public static SqlStatement ProcessCreatedUpdatedTimeQuery(this DataConnection dc, SqlStatement statement, ITimeProvider timeProvider)
    {
        return statement.QueryType switch
        {
            QueryType.Insert => dc.ProcessInsertTimeQuery(statement, timeProvider),
            QueryType.Update => dc.ProcessUpdateTimeQuery(statement, timeProvider),
            QueryType.InsertOrUpdate => dc.ProcessInsertOrUpdateTimeQuery(statement, timeProvider),
            _ => statement
        };
    }

    private static SqlStatement ProcessInsertTimeQuery(this DataConnection cn, SqlStatement statement, ITimeProvider timeProvider)
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

    private static SqlStatement ProcessUpdateTimeQuery(this DataConnection cn, SqlStatement statement, ITimeProvider timeProvider)
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

    private static SqlStatement ProcessInsertOrUpdateTimeQuery(this DataConnection cn, SqlStatement statement, ITimeProvider timeProvider)
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

    private static void AddSetColumnTimeExpression(SqlTable table, List<SqlSetExpression> expressions, ColumnDescriptor desc, Instant now)
    {
        var field = table.FindFieldByMemberName(desc.MemberName).NotNull();
        var column = expressions.FindField(field);
        if (column is null)
            expressions.Add(new SqlSetExpression(field, new SqlValue(typeof(Instant), now)));
        else
            column.Expression = new SqlValue(typeof(Instant), now);
    }

    private static void DeleteSetColumnTimeExpression(SqlTable table, List<SqlSetExpression> expressions, ColumnDescriptor desc)
    {
        var field = table.FindFieldByMemberName(desc.MemberName).NotNull();
        var column = expressions.FindField(field);
        expressions.RemoveAll(x => x == column);
    }

    private static ColumnDescriptor? FindColumn(this IEnumerable<ColumnDescriptor> columns, string name)
    {
        foreach (var column in columns)
            if (column.MemberAccessor.MemberInfo.Name == name && column is { HasValuesToSkipOnInsert: false, HasValuesToSkipOnUpdate: false })
                return column;

        return null;
    }

    private static SqlSetExpression? FindField(this IEnumerable<SqlSetExpression> expressions, SqlField field)
    {
        foreach (var expression in expressions)
            if (expression.Column is SqlField f && f != null! && f.PhysicalName == field.PhysicalName)
                return expression;

        return null;
    }

    private static SqlStatement Clone(SqlStatement original)
    {
        var clone = original.Clone(e => e.ElementType != QueryElementType.SqlParameter);

        return clone;
    }

    private static SqlTable? GetUpdateTable(SqlStatement statement)
    {
        if (statement is SqlUpdateStatement update)
            return update.GetUpdateTable();

        if (statement.SelectQuery == null)
            return null;

        if (statement.SelectQuery.From.Tables.Count > 0 &&
            statement.SelectQuery?.From.Tables[0].Source is SqlTable source)
        {
            return source;
        }

        return null;
    }
}