using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.IntegrationTesting;

/// <summary>
/// Abstraction over a started in-memory ASP.NET Core test host.
/// </summary>
public interface ITestHost : IAsyncDisposable
{
    /// <summary>
    /// Gets the underlying <see cref="TestServer"/> created by the in-memory host.
    /// </summary>
    TestServer Server { get; }

    /// <summary>
    /// Creates a new asynchronous service scope.
    /// </summary>
    /// <returns>An <see cref="AsyncServiceScope"/> for managing scoped services.</returns>
    AsyncServiceScope CreateAsyncScope();

    /// <summary>
    /// Resolves a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The resolved service instance.</returns>
    T Get<T>()
        where T : notnull;

    /// <summary>
    /// Resolves a keyed service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="key">The key for the service.</param>
    /// <returns>The resolved service instance.</returns>
    T GetKeyed<T>(object key)
        where T : notnull;
}
