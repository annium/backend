using System.Threading.Channels;
using Annium.MessageBus.Abstractions;

namespace Annium.MessageBus.InMemory.Internal;

/// <summary>
/// A single in-memory subscription: an unbounded channel plus its routing key. Consumers sharing the same
/// <see cref="Group"/> on the same subject share one instance (competing consumers); a null group yields a
/// dedicated instance per subscription (fan-out).
/// </summary>
internal sealed class InMemorySubscription
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InMemorySubscription"/> class.
    /// </summary>
    /// <param name="pattern">The compiled subject pattern.</param>
    /// <param name="key">The subscription subject (routing key for group reuse).</param>
    /// <param name="group">The consumer group, or null for fan-out.</param>
    /// <param name="delivery">The delivery mode.</param>
    public InMemorySubscription(SubjectPattern pattern, string key, string? group, DeliveryMode delivery)
    {
        Pattern = pattern;
        Key = key;
        Group = group;
        Delivery = delivery;
        _channel = Channel.CreateUnbounded<TransportMessage>();
    }

    /// <summary>
    /// The backing channel.
    /// </summary>
    private readonly Channel<TransportMessage> _channel;

    /// <summary>
    /// Gets the compiled subject pattern used for routing.
    /// </summary>
    public SubjectPattern Pattern { get; }

    /// <summary>
    /// Gets the subscription subject (routing key used to reuse a group's channel).
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the consumer group, or null for fan-out.
    /// </summary>
    public string? Group { get; }

    /// <summary>
    /// Gets the delivery mode governing redelivery on abandon.
    /// </summary>
    public DeliveryMode Delivery { get; }

    /// <summary>
    /// Gets or sets the number of consumers reading this subscription (for group refcounting).
    /// </summary>
    public int Readers { get; set; }

    /// <summary>
    /// Gets the channel writer.
    /// </summary>
    public ChannelWriter<TransportMessage> Writer => _channel.Writer;

    /// <summary>
    /// Gets the channel reader.
    /// </summary>
    public ChannelReader<TransportMessage> Reader => _channel.Reader;
}
