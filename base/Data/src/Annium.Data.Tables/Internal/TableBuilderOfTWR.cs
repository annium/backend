using System;
using System.Linq.Expressions;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;

namespace Annium.Data.Tables.Internal;

internal class TableBuilder<TR, TW> : ITableBuilder<TR, TW>
    where TR : IEquatable<TR>, ICopyable<TR>
    where TW : notnull
{
    private TablePermission _permissions;
    private Expression<Func<TW, object>>? _getKey;
    private Func<TW, bool>? _getIsActive;
    private Func<TW, TR>? _toRead;
    private readonly IMapper _mapper;
    private readonly ILogger<Table<TR, TW>> _logger;

    public TableBuilder(
        IMapper mapper,
        ILogger<Table<TR, TW>> logger
    )
    {
        _mapper = mapper;
        _logger = logger;
    }

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
    )
    {
        if (_getKey is null)
            throw new InvalidOperationException($"Table<{typeof(TR).Name},{typeof(TW).Name}> must have key");

        return new Table<TR, TW>(
            _permissions,
            _getKey,
            _getIsActive ?? (_ => true),
            _toRead ?? (x => _mapper.Map<TR>(x)),
            _logger
        );
    }
}