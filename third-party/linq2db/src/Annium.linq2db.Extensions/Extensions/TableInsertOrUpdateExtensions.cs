using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Annium.linq2db.Extensions.Configuration.Metadata;
using LinqToDB;

namespace Annium.linq2db.Extensions.Extensions;

public static class TableSaveExtensions
{
    public static Task<int> InsertAsync<T>(
        this ITable<T> table,
        T value
    )
        where T : notnull
    {
        var tableMetadata = table.GetMetadata();
        var insertSetter = BuildInsertSetter(tableMetadata, value);

        return table.InsertAsync(insertSetter, CancellationToken.None);
    }

    public static Task<int> UpdateAsync<T>(
        this ITable<T> table,
        T value
    )
        where T : notnull
    {
        var tableMetadata = table.GetMetadata();
        var updateSetter = BuildUpdateSetter<T, T>(tableMetadata, value);

        return table.UpdateAsync(updateSetter, CancellationToken.None);
    }

    public static Task<int> InsertOrUpdateAsync<T>(
        this ITable<T> table,
        T value
    )
        where T : notnull
    {
        var tableMetadata = table.GetMetadata();
        var insertSetter = BuildInsertSetter(tableMetadata, value);
        var onDuplicateKeyUpdateSetter = BuildUpdateSetter<T, T?>(tableMetadata, value);

        return table.InsertOrUpdateAsync(insertSetter, onDuplicateKeyUpdateSetter, CancellationToken.None);
    }

    private static Expression<Func<T>> BuildInsertSetter<T>(TableMetadata table, T value) => Expression.Lambda<Func<T>>(
        Expression.MemberInit(
            Expression.New(typeof(T)),
            table.Columns.Values
                .Where(c => c.Association is null)
                .Select(c => Expression.Bind(c.Member, Expression.PropertyOrField(Expression.Constant(value), c.Member.Name)))
        )
    );

    private static Expression<Func<TIn, TOut>> BuildUpdateSetter<TIn, TOut>(TableMetadata table, TIn value) => Expression.Lambda<Func<TIn, TOut>>(
        Expression.MemberInit(
            Expression.New(typeof(TIn)),
            table.Columns.Values
                .Where(c => c.Association is null)
                .Select(c => Expression.Bind(c.Member, Expression.PropertyOrField(Expression.Constant(value), c.Member.Name)))
        ),
        Expression.Parameter(typeof(TIn))
    );
}