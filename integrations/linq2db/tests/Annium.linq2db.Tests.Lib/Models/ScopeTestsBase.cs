using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Runtime.Time;
using Annium.linq2db.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.linq2db.Tests.Lib.Db.Models;
using Annium.Testing;
using LinqToDB;
using LinqToDB.Async;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Annium.linq2db.Tests.Lib.Models;

/// <summary>
/// Base class providing shared tests for the connection/transaction scope structs and the
/// service-provider extensions that create them (rollback-on-dispose, double-dispose safety,
/// disposed-state guards, and disposed-provider recovery).
/// </summary>
public class ScopeTestsBase : TestBase
{
    protected ScopeTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<LibServicePack>();
    }

    /// <summary>
    /// Verifies a transaction scope rolls its work back when disposed without an explicit commit.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task TransactionScope_RollsBackOnDispose_Base()
    {
        // arrange
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var name = Guid.NewGuid().ToString();

        // act — insert inside a transaction scope, then dispose without committing
        await using (var txnScope = Provider.GetTransactionScope<Connection>())
        {
            txnScope.ThrowIfDisposed();
            var (conn, _) = txnScope;
            await conn.Companies.InsertAsync(new Company(name, new CompanyMetadata("somewhere")));
            // visible within the transaction
            (await conn.Companies.Where(x => x.Name == name).CountAsync()).Is(1);
        }

        // assert — a fresh connection sees no row (the transaction rolled back)
        await using var verify = Provider.GetConnectionScope<Connection>();
        verify.ThrowIfDisposed();
        (await verify.Cn.Companies.Where(x => x.Name == name).CountAsync()).Is(0);
    }

    /// <summary>
    /// Verifies disposing a disposed-state transaction scope (as returned by the factory's
    /// ObjectDisposedException path) is a guarded no-op rather than throwing, for every arity.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task TransactionScope_DisposedState_DisposeIsNoop_Base()
    {
        var s1 = new TransactionScope<Connection>(true);
        s1.IsDisposed.IsTrue();
        await s1.DisposeAsync();

        var s2 = new TransactionScope<Connection, Connection>(true);
        await s2.DisposeAsync();

        var s3 = new TransactionScope<Connection, Connection, Connection>(true);
        await s3.DisposeAsync();

        var s4 = new TransactionScope<Connection, Connection, Connection, Connection>(true);
        await s4.DisposeAsync();
    }

    /// <summary>
    /// Verifies disposing a disposed-state connection scope (as returned by the factory's
    /// ObjectDisposedException path) is a guarded no-op rather than throwing, for every arity.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task ConnectionScope_DisposedState_DisposeIsNoop_Base()
    {
        var c1 = new ConnectionScope<Connection>(true);
        c1.IsDisposed.IsTrue();
        await c1.DisposeAsync();

        var c2 = new ConnectionScope<Connection, Connection>(true);
        await c2.DisposeAsync();

        var c3 = new ConnectionScope<Connection, Connection, Connection>(true);
        await c3.DisposeAsync();

        var c4 = new ConnectionScope<Connection, Connection, Connection, Connection>(true);
        await c4.DisposeAsync();
    }

    /// <summary>
    /// Verifies a disposed-state transaction scope reports disposal and guards its accessors for every arity.
    /// </summary>
    protected void TransactionScope_DisposedState_Throws_Base()
    {
        var s1 = new TransactionScope<Connection>(true);
        s1.IsDisposed.IsTrue();
        Wrap.It(() => s1.ThrowIfDisposed()).Throws<ObjectDisposedException>();
        Wrap.It(() =>
            {
                var (_, _) = s1;
            })
            .Throws<ObjectDisposedException>();

        var s2 = new TransactionScope<Connection, Connection>(true);
        s2.IsDisposed.IsTrue();
        Wrap.It(() => s2.ThrowIfDisposed()).Throws<ObjectDisposedException>();
        Wrap.It(() =>
            {
                var (_, _, _, _) = s2;
            })
            .Throws<ObjectDisposedException>();

        var s3 = new TransactionScope<Connection, Connection, Connection>(true);
        s3.IsDisposed.IsTrue();
        Wrap.It(() => s3.ThrowIfDisposed()).Throws<ObjectDisposedException>();
        Wrap.It(() =>
            {
                var (_, _, _, _, _, _) = s3;
            })
            .Throws<ObjectDisposedException>();

        var s4 = new TransactionScope<Connection, Connection, Connection, Connection>(true);
        s4.IsDisposed.IsTrue();
        Wrap.It(() => s4.ThrowIfDisposed()).Throws<ObjectDisposedException>();
        Wrap.It(() =>
            {
                var (_, _, _, _, _, _, _, _) = s4;
            })
            .Throws<ObjectDisposedException>();
    }

    /// <summary>
    /// Verifies a disposed-state connection scope reports disposal and guards its accessors for every arity.
    /// </summary>
    protected void ConnectionScope_DisposedState_Throws_Base()
    {
        var c1 = new ConnectionScope<Connection>(true);
        c1.IsDisposed.IsTrue();
        Wrap.It(() => c1.ThrowIfDisposed()).Throws<ObjectDisposedException>();

        var c2 = new ConnectionScope<Connection, Connection>(true);
        c2.IsDisposed.IsTrue();
        Wrap.It(() => c2.ThrowIfDisposed()).Throws<ObjectDisposedException>();
        Wrap.It(() =>
            {
                var (_, _) = c2;
            })
            .Throws<ObjectDisposedException>();

        var c3 = new ConnectionScope<Connection, Connection, Connection>(true);
        c3.IsDisposed.IsTrue();
        Wrap.It(() => c3.ThrowIfDisposed()).Throws<ObjectDisposedException>();
        Wrap.It(() =>
            {
                var (_, _, _) = c3;
            })
            .Throws<ObjectDisposedException>();

        var c4 = new ConnectionScope<Connection, Connection, Connection, Connection>(true);
        c4.IsDisposed.IsTrue();
        Wrap.It(() => c4.ThrowIfDisposed()).Throws<ObjectDisposedException>();
        Wrap.It(() =>
            {
                var (_, _, _, _) = c4;
            })
            .Throws<ObjectDisposedException>();
    }

    /// <summary>
    /// Verifies creating a connection scope from a disposed provider yields a disposed scope instead of throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task GetConnectionScope_DisposedProvider_ReturnsDisposedScope_Base()
    {
        var inner = Provider.CreateAsyncScope();
        var sp = inner.ServiceProvider;
        await inner.DisposeAsync();

        sp.GetConnectionScope<Connection>().IsDisposed.IsTrue();
        sp.GetConnectionScope<Connection, ConnectionB>().IsDisposed.IsTrue();
        sp.GetConnectionScope<Connection, ConnectionB, ConnectionC>().IsDisposed.IsTrue();
        sp.GetConnectionScope<Connection, ConnectionB, ConnectionC, ConnectionD>().IsDisposed.IsTrue();
    }

    /// <summary>
    /// Verifies creating a transaction scope from a disposed provider yields a disposed scope instead of throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task GetTransactionScope_DisposedProvider_ReturnsDisposedScope_Base()
    {
        var inner = Provider.CreateAsyncScope();
        var sp = inner.ServiceProvider;
        await inner.DisposeAsync();

        sp.GetTransactionScope<Connection>().IsDisposed.IsTrue();
        sp.GetTransactionScope<Connection, ConnectionB>().IsDisposed.IsTrue();
        sp.GetTransactionScope<Connection, ConnectionB, ConnectionC>().IsDisposed.IsTrue();
        sp.GetTransactionScope<Connection, ConnectionB, ConnectionC, ConnectionD>().IsDisposed.IsTrue();
    }

    /// <summary>
    /// Verifies the multi-arity connection scope overloads resolve distinct, independently usable
    /// connections against a live provider and dispose cleanly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task ConnectionScope_MultiArity_Live_Base()
    {
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());

        // 2-arity — insert via the first connection, read back via the second to prove both
        // distinct connections are resolved and operate against the same database
        await using (var s2 = Provider.GetConnectionScope<Connection, ConnectionB>())
        {
            s2.ThrowIfDisposed();
            var (c1, c2) = s2;
            var name = Guid.NewGuid().ToString();
            await c1.GetTable<Company>().InsertAsync(new Company(name, new CompanyMetadata("somewhere")));
            (await c2.GetTable<Company>().Where(x => x.Name == name).CountAsync()).Is(1);
        }

        // 3-arity — all three connections resolve to distinct, non-null instances
        await using (var s3 = Provider.GetConnectionScope<Connection, ConnectionB, ConnectionC>())
        {
            s3.ThrowIfDisposed();
            var (c1, c2, c3) = s3;
            c1.IsNotDefault();
            c2.IsNotDefault();
            c3.IsNotDefault();
        }

        // 4-arity — insert via the first connection, read back via the fourth
        await using (var s4 = Provider.GetConnectionScope<Connection, ConnectionB, ConnectionC, ConnectionD>())
        {
            s4.ThrowIfDisposed();
            var (c1, _, _, c4) = s4;
            var name = Guid.NewGuid().ToString();
            await c1.GetTable<Company>().InsertAsync(new Company(name, new CompanyMetadata("somewhere")));
            (await c4.GetTable<Company>().Where(x => x.Name == name).CountAsync()).Is(1);
        }
    }

    /// <summary>
    /// Verifies the 2-arity transaction scope rolls back both connections' work on dispose.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task TransactionScope_TwoArity_RollsBackOnDispose_Base()
    {
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var n1 = Guid.NewGuid().ToString();
        var n2 = Guid.NewGuid().ToString();

        await using (var scope = Provider.GetTransactionScope<Connection, ConnectionB>())
        {
            scope.ThrowIfDisposed();
            var (c1, _, c2, _) = scope;
            await c1.GetTable<Company>().InsertAsync(new Company(n1, new CompanyMetadata("somewhere")));
            await c2.GetTable<Company>().InsertAsync(new Company(n2, new CompanyMetadata("somewhere")));
            (await c1.GetTable<Company>().Where(x => x.Name == n1).CountAsync()).Is(1);
        }

        await using var verify = Provider.GetConnectionScope<Connection>();
        verify.ThrowIfDisposed();
        var names = new[] { n1, n2 };
        (await verify.Cn.GetTable<Company>().Where(x => names.Contains(x.Name)).CountAsync()).Is(0);
    }

    /// <summary>
    /// Verifies the 3-arity transaction scope rolls back all three connections' work on dispose.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task TransactionScope_ThreeArity_RollsBackOnDispose_Base()
    {
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var n1 = Guid.NewGuid().ToString();
        var n2 = Guid.NewGuid().ToString();
        var n3 = Guid.NewGuid().ToString();

        await using (var scope = Provider.GetTransactionScope<Connection, ConnectionB, ConnectionC>())
        {
            scope.ThrowIfDisposed();
            var (c1, _, c2, _, c3, _) = scope;
            await c1.GetTable<Company>().InsertAsync(new Company(n1, new CompanyMetadata("somewhere")));
            await c2.GetTable<Company>().InsertAsync(new Company(n2, new CompanyMetadata("somewhere")));
            await c3.GetTable<Company>().InsertAsync(new Company(n3, new CompanyMetadata("somewhere")));
            (await c1.GetTable<Company>().Where(x => x.Name == n1).CountAsync()).Is(1);
        }

        await using var verify = Provider.GetConnectionScope<Connection>();
        verify.ThrowIfDisposed();
        var names = new[] { n1, n2, n3 };
        (await verify.Cn.GetTable<Company>().Where(x => names.Contains(x.Name)).CountAsync()).Is(0);
    }

    /// <summary>
    /// Verifies the 4-arity transaction scope rolls back every connection's work on dispose.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task TransactionScope_MultiArity_RollsBackOnDispose_Base()
    {
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var n1 = Guid.NewGuid().ToString();
        var n2 = Guid.NewGuid().ToString();
        var n3 = Guid.NewGuid().ToString();
        var n4 = Guid.NewGuid().ToString();

        // act — each distinct connection inserts a row in its own transaction, then the scope is
        // disposed without committing
        await using (var scope = Provider.GetTransactionScope<Connection, ConnectionB, ConnectionC, ConnectionD>())
        {
            scope.ThrowIfDisposed();
            var (c1, _, c2, _, c3, _, c4, _) = scope;
            await c1.GetTable<Company>().InsertAsync(new Company(n1, new CompanyMetadata("somewhere")));
            await c2.GetTable<Company>().InsertAsync(new Company(n2, new CompanyMetadata("somewhere")));
            await c3.GetTable<Company>().InsertAsync(new Company(n3, new CompanyMetadata("somewhere")));
            await c4.GetTable<Company>().InsertAsync(new Company(n4, new CompanyMetadata("somewhere")));
            // visible within its own transaction
            (await c1.GetTable<Company>().Where(x => x.Name == n1).CountAsync()).Is(1);
        }

        // assert — a fresh connection sees none of the four rows (all transactions rolled back)
        await using var verify = Provider.GetConnectionScope<Connection>();
        verify.ThrowIfDisposed();
        var names = new[] { n1, n2, n3, n4 };
        (await verify.Cn.GetTable<Company>().Where(x => names.Contains(x.Name)).CountAsync()).Is(0);
    }
}
