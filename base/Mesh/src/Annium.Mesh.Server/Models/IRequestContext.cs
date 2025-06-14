// ReSharper disable once CheckNamespace
namespace Annium.Mesh.Server;

/// <summary>
/// Provides context for handling incoming requests from mesh clients.
/// </summary>
/// <typeparam name="TRequest">The type of request data.</typeparam>
public interface IRequestContext<TRequest>
{
    /// <summary>
    /// Gets the request data sent by the client.
    /// </summary>
    TRequest Request { get; }
}
