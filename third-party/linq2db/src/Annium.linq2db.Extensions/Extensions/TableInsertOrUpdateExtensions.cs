using System;
using System.Collections.Concurrent;
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
    private static readonly ConcurrentDictionary<Type, LambdaExpression> InsertSetters = new();
    private static readonly ConcurrentDictionary<Type, LambdaExpression> OnDuplicateKeyUpdateSetters = new();

    public static Task<int> InsertOrUpdateAsync<T>(
        this ITable<T> target,
        T value
    )
        where T : notnull
    {
        var table = target.DataContext.MappingSchema.Describe().Tables.SingleOrDefault(x => x.Type == typeof(T))
                    ?? throw new InvalidOperationException($"Unknown type {typeof(T).FriendlyName()}");
        var insertSetter = (Expression<Func<T>>) InsertSetters.GetOrAdd(typeof(T), static (type, tuple) => BuildInsertSetter(type, tuple), (table, value));
        var onDuplicateKeyUpdateSetter = (Expression<Func<T, T?>>) OnDuplicateKeyUpdateSetters.GetOrAdd(typeof(T), static (type, tuple) => BuildOnDuplicateKeyUpdateSetter(type, tuple), (table, value));

        return target.InsertOrUpdateAsync(insertSetter, onDuplicateKeyUpdateSetter, CancellationToken.None);
    }

    private static LambdaExpression BuildInsertSetter<T>(Type type, (TableMetadata table, T value) data) => Expression.Lambda(
        Expression.MemberInit(
            Expression.New(type),
            data.table.Columns
                .Where(c => c.Association is null)
                .Select(c => Expression.Bind(c.Member, Expression.PropertyOrField(Expression.Constant(data.value), c.Member.Name)))
        )
    );

    private static LambdaExpression BuildOnDuplicateKeyUpdateSetter<T>(Type type, (TableMetadata table, T value) data) => Expression.Lambda<Func<T, T?>>(
        Expression.MemberInit(
            Expression.New(type),
            data.table.Columns
                .Where(c => c.Association is null)
                .Select(c => Expression.Bind(c.Member, Expression.PropertyOrField(Expression.Constant(data.value), c.Member.Name)))
        ),
        Expression.Parameter(typeof(T))
    );
}