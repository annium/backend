using System.Threading;
using System.Threading.Tasks;
using NATS.Client.JetStream;

namespace Annium.MessageBus.Nats.Internal;

/// <summary>
/// Resolves the JetStream stream that captures a given subject. Stream provisioning is external — this adapter never
/// creates streams; it only validates that one exists and surfaces a clear error otherwise (so an at-least-once /
/// replay subscription against an unprovisioned subject fails fast with an actionable message rather than hanging).
/// </summary>
internal static class NatsStreamValidator
{
    /// <summary>
    /// Finds the name of the stream whose configured subjects capture <paramref name="subject"/>.
    /// </summary>
    /// <param name="jetStream">The JetStream context.</param>
    /// <param name="subject">The subject (or subscription pattern) to resolve a stream for.</param>
    /// <param name="ct">A token to cancel the lookup.</param>
    /// <returns>The resolved stream name.</returns>
    /// <exception cref="NatsJSException">Thrown when no stream captures the subject (external provisioning missing).</exception>
    public static async ValueTask<string> ResolveStreamAsync(
        NatsJSContext jetStream,
        string subject,
        CancellationToken ct
    )
    {
        await foreach (var stream in jetStream.ListStreamsAsync(subject, cancellationToken: ct))
            return stream.Info.Config.Name!;

        throw new NatsJSException(
            $"No JetStream stream is provisioned for subject '{subject}'. At-least-once and replay subscriptions "
                + "require a pre-existing stream capturing the subject (this adapter does not create streams)."
        );
    }
}
