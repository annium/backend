using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.IntegrationTesting;

public interface ITestHost : IAsyncDisposable
{
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
