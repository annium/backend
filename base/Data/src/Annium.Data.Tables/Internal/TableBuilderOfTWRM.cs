using System;
using System.Linq.Expressions;
using Annium.Core.Mapper;
using Annium.Core.Primitives;

namespace Annium.Data.Tables.Internal;

internal class TableBuilder<TR, TW, TM> : ITableBuilder<TR, TW, TM>
    where TR : IEquatable<TR>, ICopyable<TR>
    where TW : notnull
    where TM : notnull
{
    private readonly TM _mappingContext;
    private TablePermission _permissions;
    private Expression<Func<TW, object>>? _getKey;
    private Func<TW, bool>? _getIsActive;

    public TableBuilder(
        TM mappingContext,
        TablePermission permissions,
        Expression<Func<TW, object>>? getKey,
        Func<TW, bool>? getIsActive
    )
    {
        _mappingContext = mappingContext;
        _permissions = permissions;
        _getKey = getKey;
        _getIsActive = getIsActive;
    }

    public ITableBuilder<TR, TW, TM> Allow(TablePermission permissions)
    {
        _permissions = permissions;

        return this;
    }

    public ITableBuilder<TR, TW, TM> Key(Expression<Func<TW, object>> getKey)
    {
        _getKey = getKey;

        return this;
    }

    public ITableBuilder<TR, TW, TM> Keep(Func<TW, bool> isActive)
    {
        _getIsActive = isActive;

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
            x => mapper.Map<TR>((x, _mappingContext))
        );
    }
}