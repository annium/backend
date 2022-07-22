using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Configuration.Metadata;
using LinqToDB;

namespace Annium.linq2db.Extensions.Extensions;

public static class TableInsertOrUpdateExtensions
{
    public static Task<int> InsertOrUpdateAsync<T>(
        this ITable<T> target,
        T value
    )
        where T : notnull
    {
        var table = target.DataContext.MappingSchema.Describe().Tables.SingleOrDefault(x => x.Type == typeof(T))
                    ?? throw new InvalidOperationException($"Unknown type {typeof(T).FriendlyName()}");
        var insertSetter = BuildInsertSetter(table, value);
        var onDuplicateKeyUpdateSetter = BuildOnDuplicateKeyUpdateSetter(table, value);

        return target.InsertOrUpdateAsync(insertSetter, onDuplicateKeyUpdateSetter, CancellationToken.None);
    }

    private static Expression<Func<T>> BuildInsertSetter<T>(TableMetadata table, T value) => Expression.Lambda<Func<T>>(
        Expression.MemberInit(
            Expression.New(typeof(T)),
            table.Columns
                .Where(c => c.Association is null)
                .Select(c => Expression.Bind(c.Member, Expression.PropertyOrField(Expression.Constant(value), c.Member.Name)))
        )
    );

    private static Expression<Func<T, T?>> BuildOnDuplicateKeyUpdateSetter<T>(TableMetadata table, T value) => Expression.Lambda<Func<T, T?>>(
        Expression.MemberInit(
            Expression.New(typeof(T)),
            table.Columns
                .Where(c => c.Association is null)
                .Select(c => Expression.Bind(c.Member, Expression.PropertyOrField(Expression.Constant(value), c.Member.Name)))
        ),
        Expression.Parameter(typeof(T))
    );
}