using Annium.Logging;

namespace Annium.Data.Tables.Internal;

internal class TableFactory : ITableFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public TableFactory(
        ILoggerFactory loggerFactory
    )
    {
        _loggerFactory = loggerFactory;
    }

    public ITableBuilder<T> New<T>()
        where T : notnull
    {
        return new TableBuilder<T>(_loggerFactory.Get<Table<T>>());
    }
}