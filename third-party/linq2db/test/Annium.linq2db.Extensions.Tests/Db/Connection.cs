using Annium.linq2db.Extensions.Models;
using Annium.linq2db.Extensions.Tests.Db.Models;
using Annium.Logging.Abstractions;
using LinqToDB;

namespace Annium.linq2db.Extensions.Tests.Db;

internal sealed class Connection : DataConnectionBase, ILogSubject<Connection>
{
    public ILogger<Connection> Logger { get; }
    public ITable<Company> Companies { get; }
    public ITable<Employee> Employees { get; }

    public Connection(
        Config<Connection> config,
        ILogger<Connection> logger
    ) : base(config.Options)
    {
        Logger = logger;
        Companies = this.GetTable<Company>();
        Employees = this.GetTable<Employee>();
    }
}