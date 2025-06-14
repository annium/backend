using Annium.Architecture.Base;
using Annium.Data.Operations;

// ReSharper disable once CheckNamespace
namespace Annium.Mesh.Server;

/// <summary>
/// Provides context for managing subscriptions where clients can receive continuous updates after initialization.
/// </summary>
/// <typeparam name="TInit">The type of initialization data for the subscription.</typeparam>
/// <typeparam name="TMessage">The type of messages sent to subscribers.</typeparam>
public interface ISubscriptionContext<TInit, TMessage> : IRequestContext<TInit>
{
    /// <summary>
    /// Handles the result of subscription initialization, indicating success or failure.
    /// </summary>
    /// <param name="result">The status result of the subscription initialization.</param>
    void Handle(IStatusResult<OperationStatus> result);

    /// <summary>
    /// Sends a message to the subscribed client.
    /// </summary>
    /// <param name="message">The message to send to the subscriber.</param>
    void Send(TMessage message);
}
