using System;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;

namespace Annium.Data.Tables.Internal;

internal class TableFactory : ITableFactory
{
    private readonly IMapper _mapper;
    private readonly ILoggerFactory _loggerFactory;

    public TableFactory(
        IMapper mapper,
        ILoggerFactory loggerFactory
    )
    {
        _mapper = mapper;
        _loggerFactory = loggerFactory;
    }

    public ITableBuilder<TR, TW> New<TR, TW>()
        where TR : IEquatable<TR>, ICopyable<TR>
        where TW : notnull
    {
        return new TableBuilder<TR, TW>(_mapper, _loggerFactory.GetLogger<Table<TR, TW>>());
    }

    public ITableBuilder<T> New<T>()
        where T : IEquatable<T>, ICopyable<T>
    {
        return new TableBuilder<T>(_loggerFactory.GetLogger<Table<T>>());
    }
}