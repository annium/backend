using System;

namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// In-process retry policy applied to a failed (Nacked with requeue) message before it is dead-lettered.
/// Uses exponential backoff.
/// </summary>
public sealed record RetryPolicy
{
    /// <summary>
    /// Gets the maximum number of processing attempts (including the first). A value of 1 disables retries.
    /// </summary>
    public int MaxAttempts { get; init; } = 5;

    /// <summary>
    /// Gets the base delay before the first retry.
    /// </summary>
    public TimeSpan BaseDelay { get; init; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Gets the multiplier applied to the delay after each attempt.
    /// </summary>
    public double Factor { get; init; } = 2.0;

    /// <summary>
    /// Gets the upper bound on the delay between attempts.
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets a value indicating whether random jitter is added to each delay to avoid thundering herds.
    /// </summary>
    public bool Jitter { get; init; } = true;

    /// <summary>
    /// Gets the default policy (5 attempts, 200 ms base, factor 2, 30 s cap, jitter on).
    /// </summary>
    public static RetryPolicy Default { get; } = new();

    /// <summary>
    /// Gets a policy that disables retries (single attempt, then dead-letter).
    /// </summary>
    public static RetryPolicy None { get; } = new() { MaxAttempts = 1 };
}
