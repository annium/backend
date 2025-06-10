using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Annium.linq2db.Extensions.Configuration.Metadata;
using Annium.Reflection.Members;
using LinqToDB;

namespace Annium.linq2db.Extensions.Extensions;

/// <summary>
/// Extension methods for table insert, update, and insert-or-update operations
/// </summary>
public static class TableSaveExtensions
{
    /// <summary>
    /// Inserts a single entity into the table
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="table">The table to insert into</param>
    /// <param name="value">The entity to insert</param>
    /// <returns>The number of affected rows</returns>
    public static Task<int> InsertAsync<T>(this ITable<T> table, T value)
        where T : notnull
    {
        var tableMetadata = table.GetMetadata();

        var insertSetter = BuildInsertSetter(tableMetadata, value);

        return table.InsertAsync(insertSetter, CancellationToken.None);
    }

    /// <summary>
    /// Updates a single entity in the table based on its primary key
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="table">The table to update</param>
    /// <param name="value">The entity to update</param>
    /// <returns>The number of affected rows</returns>
    public static Task<int> UpdateAsync<T>(this ITable<T> table, T value)
        where T : notnull
    {
        var tableMetadata = table.GetMetadata();

        var primaryKeyPredicate = BuildPrimaryKeyPredicate(tableMetadata, value);
        var updateSetter = BuildUpdateSetter<T, T>(tableMetadata, value);

        return table.Where(primaryKeyPredicate).UpdateAsync(updateSetter, CancellationToken.None);
    }

    /// <summary>
    /// Inserts or updates a single entity (upsert operation)
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="table">The table to operate on</param>
    /// <param name="value">The entity to insert or update</param>
    /// <returns>The number of affected rows</returns>
    public static Task<int> InsertOrUpdateAsync<T>(this ITable<T> table, T value)
        where T : notnull
    {
        var tableMetadata = table.GetMetadata();

        var insertSetter = BuildInsertSetter(tableMetadata, value);
        var onDuplicateKeyUpdateSetter = BuildUpdateSetter<T, T?>(tableMetadata, value);

        return table.InsertOrUpdateAsync(insertSetter, onDuplicateKeyUpdateSetter, CancellationToken.None);
    }

    /// <summary>
    /// Builds an expression that creates an entity with values for insert operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="table">The table metadata.</param>
    /// <param name="value">The entity value to extract data from.</param>
    /// <returns>An expression that creates an entity with the appropriate values.</returns>
    private static Expression<Func<T>> BuildInsertSetter<T>(TableMetadata table, T value)
        where T : notnull
    {
        var bindings = table
            .Columns.Values.Where(c => c.Association is null)
            .Select<ColumnMetadata, MemberBinding>(c =>
            {
                var memberValue = c.Member.GetPropertyOrFieldValue(value);
                return Expression.Bind(c.Member, Expression.Constant(memberValue, c.Type));
            })
            .ToArray();

        return Expression.Lambda<Func<T>>(Expression.MemberInit(Expression.New(typeof(T)), bindings));
    }

    /// <summary>
    /// Builds a predicate expression that matches entities by their primary key values.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="table">The table metadata.</param>
    /// <param name="value">The entity value to extract primary key values from.</param>
    /// <returns>A predicate expression that matches entities with the same primary key values.</returns>
    private static Expression<Func<T, bool>> BuildPrimaryKeyPredicate<T>(TableMetadata table, T value)
        where T : notnull
    {
        var param = Expression.Parameter(typeof(T));

        var condition = table
            .Columns.Values.Where(c => c.PrimaryKey is not null)
            .Select(c =>
            {
                var memberValue = c.Member.GetPropertyOrFieldValue(value);
                return Expression.Equal(
                    Expression.PropertyOrField(param, c.Member.Name),
                    Expression.Constant(memberValue, c.Type)
                );
            })
            .Aggregate(Expression.AndAlso);

        return Expression.Lambda<Func<T, bool>>(condition, param);
    }

    /// <summary>
    /// Builds an expression that creates an entity with values for update operations, excluding primary key columns.
    /// </summary>
    /// <typeparam name="TIn">The input entity type.</typeparam>
    /// <typeparam name="TOut">The output entity type.</typeparam>
    /// <param name="table">The table metadata.</param>
    /// <param name="value">The entity value to extract data from.</param>
    /// <returns>An expression that creates an entity with the appropriate values for updates.</returns>
    private static Expression<Func<TIn, TOut>> BuildUpdateSetter<TIn, TOut>(TableMetadata table, TIn value)
        where TIn : notnull
    {
        var bindings = table
            .Columns.Values.Where(c => c.Association is null && !c.Attribute.IsPrimaryKey)
            .Select<ColumnMetadata, MemberBinding>(c =>
            {
                var memberValue = c.Member.GetPropertyOrFieldValue(value);
                return Expression.Bind(c.Member, Expression.Constant(memberValue, c.Type));
            })
            .ToArray();

        return Expression.Lambda<Func<TIn, TOut>>(
            Expression.MemberInit(Expression.New(typeof(TIn)), bindings),
            Expression.Parameter(typeof(TIn))
        );
    }
}
