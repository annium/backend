using System;
using System.Data;
using Annium.Core.DependencyInjection.Extensions;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.linq2db.Extensions.Extensions;

/// <summary>
/// Extension methods for IServiceProvider to create transaction scopes for database operations.
/// </summary>
public static class ServiceProviderTransactionScopeExtensions
{
    /// <summary>
    /// Creates a transaction scope for a single DataConnection type.
    /// </summary>
    /// <typeparam name="T">The DataConnection type.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <returns>A transaction scope wrapping the DataConnection, transaction, and service scope.</returns>
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

    /// <summary>
    /// Creates a transaction scope for two DataConnection types.
    /// </summary>
    /// <typeparam name="T1">The first DataConnection type.</typeparam>
    /// <typeparam name="T2">The second DataConnection type.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <returns>A transaction scope wrapping both DataConnections, their transactions, and service scope.</returns>
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

    /// <summary>
    /// Creates a transaction scope for three DataConnection types.
    /// </summary>
    /// <typeparam name="T1">The first DataConnection type.</typeparam>
    /// <typeparam name="T2">The second DataConnection type.</typeparam>
    /// <typeparam name="T3">The third DataConnection type.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <returns>A transaction scope wrapping all three DataConnections, their transactions, and service scope.</returns>
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

    /// <summary>
    /// Creates a transaction scope for four DataConnection types.
    /// </summary>
    /// <typeparam name="T1">The first DataConnection type.</typeparam>
    /// <typeparam name="T2">The second DataConnection type.</typeparam>
    /// <typeparam name="T3">The third DataConnection type.</typeparam>
    /// <typeparam name="T4">The fourth DataConnection type.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <returns>A transaction scope wrapping all four DataConnections, their transactions, and service scope.</returns>
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
