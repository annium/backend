using System;
using System.Threading.Tasks;

namespace Annium.linq2db.Extensions;

public abstract class RepositoryBase<TConnection> : IAsyncDisposable
    where TConnection : DataConnectionBase
{
    protected readonly TConnection Db;

    protected RepositoryBase(TConnection db)
    {
        Db = db;
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
    }
}