using System.Linq;
using Annium.MessageBus.Abstractions;

namespace Annium.MessageBus.RabbitMq.Internal;

/// <summary>
/// Translates canonical subjects/patterns into RabbitMQ topic routing keys and derives queue names. A concrete subject
/// is used verbatim as the routing key (dots already separate tokens); a canonical wildcard maps to the RabbitMQ topic
/// syntax — <c>*</c> (one token) stays <c>*</c>, and <c>&gt;</c> (trailing tokens) becomes <c>#</c>. See feature spec
/// §8.2.3.
/// </summary>
internal static class RoutingKeyTranslator
{
    /// <summary>
    /// Builds the RabbitMQ binding key for a subscription subject: the literal subject when concrete, otherwise the
    /// canonical wildcard translated to RabbitMQ topic syntax (<c>*</c> → <c>*</c>, <c>&gt;</c> → <c>#</c>).
    /// </summary>
    /// <param name="subject">The concrete or wildcard subject.</param>
    /// <returns>The RabbitMQ binding key.</returns>
    public static string BindingKey(string subject)
    {
        if (Subject.IsValid(subject))
            return subject;

        var parsed = SubjectPattern.Parse(subject);
        var tokens = parsed.Tokens.Select(token =>
            token switch
            {
                "*" => "*",
                ">" => "#",
                _ => token,
            }
        );
        return string.Join('.', tokens);
    }

    /// <summary>
    /// Builds the shared-queue name for a competing consumer group on a subject. Scoping the queue by both group and
    /// subject keeps different subjects independent even when they share a group name.
    /// </summary>
    /// <param name="group">The consumer group.</param>
    /// <param name="subject">The subscription subject.</param>
    /// <returns>The queue name.</returns>
    public static string QueueName(string group, string subject) => $"{group}.{subject}";
}
