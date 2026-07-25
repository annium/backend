namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Transport SPI for creating per-subscription consumers. Implemented by each adapter; consumed by the shared
/// subscriber, which creates one <see cref="ITransportConsumer"/> per <c>SubscribeAsync</c> call.
/// </summary>
public interface ITransportConsumerFactory
{
    /// <summary>
    /// Creates a consumer bound to the given subscription options (subject, group, delivery mode, flow control).
    /// </summary>
    /// <param name="options">The subscription options.</param>
    /// <returns>A new transport consumer.</returns>
    ITransportConsumer CreateConsumer(SubscriptionOptions options);
}
