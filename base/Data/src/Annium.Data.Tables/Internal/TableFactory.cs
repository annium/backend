using System;
using Annium.Core.Mapper;
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

    public ITableBuilder<T> New<T>()
        where T : IEquatable<T>
    {
        return new TableBuilder<T>(_loggerFactory.Get<Table<T>>());
    }
}