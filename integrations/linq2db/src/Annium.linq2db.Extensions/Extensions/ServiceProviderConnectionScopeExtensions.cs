using System;
using Annium.Core.DependencyInjection.Extensions;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.linq2db.Extensions.Extensions;

public static class ServiceProviderConnectionScopeExtensions
{
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
