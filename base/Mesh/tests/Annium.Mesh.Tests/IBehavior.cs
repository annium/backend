using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Mesh.Tests;

/// <summary>
/// Defines the contract for test behaviors that configure and run mesh servers for testing purposes.
/// </summary>
public interface IBehavior : IAsyncDisposable
{
    /// <summary>
    /// Registers services required for the behavior in the dependency injection container.
    /// </summary>
    /// <param name="container">The service container to register services in.</param>
    static abstract void Register(IServiceContainer container);

    /// <summary>
    /// Initializes behavior instance
    /// </summary>
    ValueTask InitializeAsync();
}
