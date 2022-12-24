using System;
using System.Linq.Expressions;
using Annium.Logging.Abstractions;

namespace Annium.Data.Tables.Internal;

internal class TableBuilder<T> : ITableBuilder<T>
{
    private TablePermission _permissions;
    private Expression<Func<T, object>>? _getKey;
    private Func<T, T, bool>? _update;
    private Func<T, bool>? _isActive;
    private readonly ILogger<Table<T>> _logger;

    public TableBuilder(
        ILogger<Table<T>> logger
    )
    {
        _logger = logger;
    }

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

    public ITableBuilder<T> UpdateWith(Func<T, T, bool> update)
    {
        _update = update;

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

        var getKey = TableHelper.BuildGetKey(_getKey);
        var update = _update ?? TableHelper.BuildUpdate<T>(_permissions);
        var isActive = _isActive ?? (_ => true);

        return new Table<T>(_permissions, getKey, update, isActive, _logger);
    }
}