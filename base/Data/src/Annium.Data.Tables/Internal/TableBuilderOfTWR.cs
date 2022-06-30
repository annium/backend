using System;
using System.Linq.Expressions;
using Annium.Core.Mapper;
using Annium.Core.Primitives;

namespace Annium.Data.Tables.Internal;

internal class TableBuilder<TR, TW> : ITableBuilder<TR, TW>
    where TR : IEquatable<TR>, ICopyable<TR>
    where TW : notnull
{
    private TablePermission _permissions;
    private Expression<Func<TW, object>>? _getKey;
    private Func<TW, bool>? _getIsActive;
    private Func<TW, TR>? _toRead;

    public ITableBuilder<TR, TW> Allow(TablePermission permissions)
    {
        _permissions = permissions;

        return this;
    }

    public ITableBuilder<TR, TW> Key(Expression<Func<TW, object>> getKey)
    {
        _getKey = getKey;

        return this;
    }

    public ITableBuilder<TR, TW> Keep(Func<TW, bool> isActive)
    {
        _getIsActive = isActive;

        return this;
    }

    public ITableBuilder<TR, TW> MapWith(Func<TW, TR> toRead)
    {
        _toRead = toRead;

        return this;
    }

    public ITable<TR, TW> Build(
        IMapper mapper
    )
    {
        if (_getKey is null)
            throw new InvalidOperationException($"Table<{typeof(TR).Name},{typeof(TW).Name}> must have key");

        return new Table<TR, TW>(
            _permissions,
            _getKey,
            _getIsActive ?? (_ => true),
            _toRead ?? (x => mapper.Map<TR>(x))
        );
    }
}