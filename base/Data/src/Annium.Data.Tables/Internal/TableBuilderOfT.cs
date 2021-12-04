using System;
using System.Linq.Expressions;
using Annium.Core.Primitives;

namespace Annium.Data.Tables.Internal;

internal class TableBuilder<T> : ITableBuilder<T>
    where T : IEquatable<T>, ICopyable<T>
{
    private TablePermission _permissions;
    private Expression<Func<T, object>>? _getKey;
    private Func<T, bool>? _isActive;

    public ITableBuilder<T> Allow(TablePermission permissions)
    {
        _permissions = permissions;

        return this;
    }

    public ITableBuilder<T> Key(Expression<Func<T, object>> getKey)
    {
        _getKey = getKey;

        return this;
    }

    public ITableBuilder<T> Keep(Func<T, bool> isActive)
    {
        _isActive = isActive;

        return this;
    }

    public ITable<T> Build(
    )
    {
        if (_getKey is null)
            throw new InvalidOperationException($"Table<{typeof(T).Name},{typeof(T).Name}> must have key");

        return new Table<T>(
            _permissions,
            _getKey,
            _isActive ?? (_ => true)
        );
    }
}