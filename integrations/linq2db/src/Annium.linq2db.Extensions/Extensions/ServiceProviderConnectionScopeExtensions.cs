using System;
using Annium.Core.DependencyInjection;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.linq2db.Extensions.Extensions;

/// <summary>
/// Extension methods for IServiceProvider to create connection scopes for database operations.
/// </summary>
public static class ServiceProviderConnectionScopeExtensions
{
    /// <summary>
    /// Creates a connection scope for a single DataConnection type.
    /// </summary>
    /// <typeparam name="T">The DataConnection type.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <returns>A connection scope wrapping the DataConnection and its service scope.</returns>
    public static ConnectionScope<T> GetConnectionScope<T>(this IServiceProvider sp)
        where T : DataConnection
    {
        try
        {
            var scope = sp.CreateAsyncScope();
            var cn = scope.ServiceProvider.Resolve<T>();

            return new ConnectionScope<T>(scope, cn);
        }
        catch (ObjectDisposedException)
        {
            return new ConnectionScope<T>(true);
        }
    }

    /// <summary>
    /// Creates a connection scope for two DataConnection types.
    /// </summary>
    /// <typeparam name="T1">The first DataConnection type.</typeparam>
    /// <typeparam name="T2">The second DataConnection type.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <returns>A connection scope wrapping both DataConnections and their service scope.</returns>
    public static ConnectionScope<T1, T2> GetConnectionScope<T1, T2>(this IServiceProvider sp)
        where T1 : DataConnection
        where T2 : DataConnection
    {
        try
        {
            var scope = sp.CreateAsyncScope();
            var cn1 = scope.ServiceProvider.Resolve<T1>();
            var cn2 = scope.ServiceProvider.Resolve<T2>();

            return new ConnectionScope<T1, T2>(scope, cn1, cn2);
        }
        catch (ObjectDisposedException)
        {
            return new ConnectionScope<T1, T2>(true);
        }
    }

    /// <summary>
    /// Creates a connection scope for three DataConnection types.
    /// </summary>
    /// <typeparam name="T1">The first DataConnection type.</typeparam>
    /// <typeparam name="T2">The second DataConnection type.</typeparam>
    /// <typeparam name="T3">The third DataConnection type.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <returns>A connection scope wrapping all three DataConnections and their service scope.</returns>
    public static ConnectionScope<T1, T2, T3> GetConnectionScope<T1, T2, T3>(this IServiceProvider sp)
        where T1 : DataConnection
        where T2 : DataConnection
        where T3 : DataConnection
    {
        try
        {
            var scope = sp.CreateAsyncScope();
            var cn1 = scope.ServiceProvider.Resolve<T1>();
            var cn2 = scope.ServiceProvider.Resolve<T2>();
            var cn3 = scope.ServiceProvider.Resolve<T3>();

            return new ConnectionScope<T1, T2, T3>(scope, cn1, cn2, cn3);
        }
        catch (ObjectDisposedException)
        {
            return new ConnectionScope<T1, T2, T3>(true);
        }
    }

    /// <summary>
    /// Creates a connection scope for four DataConnection types.
    /// </summary>
    /// <typeparam name="T1">The first DataConnection type.</typeparam>
    /// <typeparam name="T2">The second DataConnection type.</typeparam>
    /// <typeparam name="T3">The third DataConnection type.</typeparam>
    /// <typeparam name="T4">The fourth DataConnection type.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <returns>A connection scope wrapping all four DataConnections and their service scope.</returns>
    public static ConnectionScope<T1, T2, T3, T4> GetConnectionScope<T1, T2, T3, T4>(this IServiceProvider sp)
        where T1 : DataConnection
        where T2 : DataConnection
        where T3 : DataConnection
        where T4 : DataConnection
    {
        try
        {
            var scope = sp.CreateAsyncScope();
            var cn1 = scope.ServiceProvider.Resolve<T1>();
            var cn2 = scope.ServiceProvider.Resolve<T2>();
            var cn3 = scope.ServiceProvider.Resolve<T3>();
            var cn4 = scope.ServiceProvider.Resolve<T4>();

            return new ConnectionScope<T1, T2, T3, T4>(scope, cn1, cn2, cn3, cn4);
        }
        catch (ObjectDisposedException)
        {
            return new ConnectionScope<T1, T2, T3, T4>(true);
        }
    }
}
