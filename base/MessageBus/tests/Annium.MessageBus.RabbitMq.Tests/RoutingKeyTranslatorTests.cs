using Annium.MessageBus.RabbitMq.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// Unit tests for <see cref="RoutingKeyTranslator"/> (subject/pattern → RabbitMQ topic key, queue naming). Pure (no
/// broker).
/// </summary>
public class RoutingKeyTranslatorTests
{
    /// <summary>
    /// A concrete subject is used verbatim as the binding key.
    /// </summary>
    /// <param name="subject">The concrete subject.</param>
    [Theory]
    [InlineData("orders.created")]
    [InlineData("orders")]
    [InlineData("a.b.c.d")]
    public void BindingKey_Literal_IsVerbatim(string subject)
    {
        RoutingKeyTranslator.BindingKey(subject).Is(subject);
    }

    /// <summary>
    /// Canonical wildcards map to RabbitMQ topic syntax: <c>*</c> stays <c>*</c>, <c>&gt;</c> becomes <c>#</c>.
    /// </summary>
    /// <param name="pattern">The canonical wildcard pattern.</param>
    /// <param name="expected">The expected RabbitMQ binding key.</param>
    [Theory]
    [InlineData("orders.*.created", "orders.*.created")]
    [InlineData("orders.>", "orders.#")]
    [InlineData(">", "#")]
    [InlineData("a.*.b.>", "a.*.b.#")]
    [InlineData("orders.*", "orders.*")]
    public void BindingKey_Wildcard_MapsToTopicSyntax(string pattern, string expected)
    {
        RoutingKeyTranslator.BindingKey(pattern).Is(expected);
    }

    /// <summary>
    /// A group queue name is scoped by both group and subject.
    /// </summary>
    [Fact]
    public void QueueName_ScopesByGroupAndSubject()
    {
        RoutingKeyTranslator.QueueName("workers", "orders.created").Is("workers.orders.created");
    }
}
