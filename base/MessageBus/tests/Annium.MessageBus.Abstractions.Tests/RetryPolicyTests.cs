using System;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for <see cref="RetryPolicy"/> defaults.
/// </summary>
public class RetryPolicyTests
{
    /// <summary>
    /// The default policy exposes the documented backoff parameters.
    /// </summary>
    [Fact]
    public void Default_HasExpectedValues()
    {
        var policy = RetryPolicy.Default;
        policy.MaxAttempts.Is(5);
        policy.BaseDelay.Is(TimeSpan.FromMilliseconds(200));
        policy.Factor.Is(2.0);
        policy.MaxDelay.Is(TimeSpan.FromSeconds(30));
        policy.Jitter.Is(true);
    }

    /// <summary>
    /// The none policy disables retries with a single attempt.
    /// </summary>
    [Fact]
    public void None_DisablesRetries()
    {
        RetryPolicy.None.MaxAttempts.Is(1);
    }
}
