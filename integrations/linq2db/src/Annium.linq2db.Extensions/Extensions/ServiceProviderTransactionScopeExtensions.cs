using System;
using System.Data;
using Annium.Core.DependencyInjection.Extensions;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.linq2db.Extensions.Extensions;

public static class ServiceProviderTransactionScopeExtensions
{
    public static TransactionScope<T> GetTransactionScope<T>(
        this IServiceProvider sp,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
    )
        where T : DataConnection
    {
        try
        {
            var scope = sp.CreateAsyncScope();
            var cn = scope.ServiceProvider.Resolve<T>();
            var txn = cn.BeginTransaction(isolationLevel);

            return new TransactionScope<T>(scope, cn, txn);
        }
        catch (ObjectDisposedException)
        {
            return new TransactionScope<T>(true);
        }
    }

    public static TransactionScope<T1, T2> GetTransactionScope<T1, T2>(
        this IServiceProvider sp,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
    )
        where T1 : DataConnection
        where T2 : DataConnection
    {
        try
        {
            var scope = sp.CreateAsyncScope();
            var cn1 = scope.ServiceProvider.Resolve<T1>();
            var txn1 = cn1.BeginTransaction(isolationLevel);
            var cn2 = scope.ServiceProvider.Resolve<T2>();
            var txn2 = cn2.BeginTransaction(isolationLevel);

            return new TransactionScope<T1, T2>(scope, cn1, txn1, cn2, txn2);
        }
        catch (ObjectDisposedException)
        {
            return new TransactionScope<T1, T2>(true);
        }
    }

    public static TransactionScope<T1, T2, T3> GetTransactionScope<T1, T2, T3>(
        this IServiceProvider sp,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
    )
        where T1 : DataConnection
        where T2 : DataConnection
        where T3 : DataConnection
    {
        try
        {
            var scope = sp.CreateAsyncScope();
            var cn1 = scope.ServiceProvider.Resolve<T1>();
            var txn1 = cn1.BeginTransaction(isolationLevel);
            var cn2 = scope.ServiceProvider.Resolve<T2>();
            var txn2 = cn2.BeginTransaction(isolationLevel);
            var cn3 = scope.ServiceProvider.Resolve<T3>();
            var txn3 = cn3.BeginTransaction(isolationLevel);

            return new TransactionScope<T1, T2, T3>(scope, cn1, txn1, cn2, txn2, cn3, txn3);
        }
        catch (ObjectDisposedException)
        {
            return new TransactionScope<T1, T2, T3>(true);
        }
    }

    public static TransactionScope<T1, T2, T3, T4> GetTransactionScope<T1, T2, T3, T4>(
        this IServiceProvider sp,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
    )
        where T1 : DataConnection
        where T2 : DataConnection
        where T3 : DataConnection
        where T4 : DataConnection
    {
        try
        {
            var scope = sp.CreateAsyncScope();
            var cn1 = scope.ServiceProvider.Resolve<T1>();
            var txn1 = cn1.BeginTransaction(isolationLevel);
            var cn2 = scope.ServiceProvider.Resolve<T2>();
            var txn2 = cn2.BeginTransaction(isolationLevel);
            var cn3 = scope.ServiceProvider.Resolve<T3>();
            var txn3 = cn3.BeginTransaction(isolationLevel);
            var cn4 = scope.ServiceProvider.Resolve<T4>();
            var txn4 = cn4.BeginTransaction(isolationLevel);

            return new TransactionScope<T1, T2, T3, T4>(scope, cn1, txn1, cn2, txn2, cn3, txn3, cn4, txn4);
        }
        catch (ObjectDisposedException)
        {
            return new TransactionScope<T1, T2, T3, T4>(true);
        }
    }
}
