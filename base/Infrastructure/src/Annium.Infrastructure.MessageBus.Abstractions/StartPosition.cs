using System;

namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// Where a replay-capable subscription starts consuming from. Closed union — construct via the factory members
/// and consume via <see cref="Match{T}"/> / <see cref="Switch"/> (concrete cases are intentionally private).
/// Only meaningful for transports implementing <see cref="ISupportsReplay"/>.
/// </summary>
public abstract record StartPosition
{
    private StartPosition() { }

    /// <summary>
    /// Gets a position that consumes only messages produced after subscription (no history).
    /// </summary>
    public static StartPosition New { get; } = new NewPosition();

    /// <summary>
    /// Gets a position that consumes from the earliest retained message.
    /// </summary>
    public static StartPosition Earliest { get; } = new EarliestPosition();

    /// <summary>
    /// Creates a position that consumes from the first message at or after the given timestamp.
    /// </summary>
    /// <param name="timestamp">The timestamp to start from.</param>
    /// <returns>A timestamp-based start position.</returns>
    public static StartPosition FromTimestamp(DateTimeOffset timestamp) => new TimestampPosition(timestamp);

    /// <summary>
    /// Creates a position that consumes from the given transport sequence/offset.
    /// </summary>
    /// <param name="value">The sequence number or offset to start from.</param>
    /// <returns>A position-based start position.</returns>
    public static StartPosition FromPosition(long value) => new PositionPosition(value);

    /// <summary>
    /// Deconstructs this position into one of its cases, returning a value. Exhaustive by construction.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="onNew">Called for the "new messages only" case.</param>
    /// <param name="onEarliest">Called for the "earliest retained" case.</param>
    /// <param name="onTimestamp">Called for the timestamp case, with its timestamp.</param>
    /// <param name="onPosition">Called for the sequence/offset case, with its value.</param>
    /// <returns>The value produced by the matching handler.</returns>
    public abstract T Match<T>(
        Func<T> onNew,
        Func<T> onEarliest,
        Func<DateTimeOffset, T> onTimestamp,
        Func<long, T> onPosition
    );

    /// <summary>
    /// Deconstructs this position into one of its cases, performing a side effect. Exhaustive by construction.
    /// </summary>
    /// <param name="onNew">Called for the "new messages only" case.</param>
    /// <param name="onEarliest">Called for the "earliest retained" case.</param>
    /// <param name="onTimestamp">Called for the timestamp case, with its timestamp.</param>
    /// <param name="onPosition">Called for the sequence/offset case, with its value.</param>
    public void Switch(Action onNew, Action onEarliest, Action<DateTimeOffset> onTimestamp, Action<long> onPosition) =>
        Match<object?>(
            () =>
            {
                onNew();
                return null;
            },
            () =>
            {
                onEarliest();
                return null;
            },
            timestamp =>
            {
                onTimestamp(timestamp);
                return null;
            },
            value =>
            {
                onPosition(value);
                return null;
            }
        );

    private sealed record NewPosition : StartPosition
    {
        public override T Match<T>(
            Func<T> onNew,
            Func<T> onEarliest,
            Func<DateTimeOffset, T> onTimestamp,
            Func<long, T> onPosition
        ) => onNew();
    }

    private sealed record EarliestPosition : StartPosition
    {
        public override T Match<T>(
            Func<T> onNew,
            Func<T> onEarliest,
            Func<DateTimeOffset, T> onTimestamp,
            Func<long, T> onPosition
        ) => onEarliest();
    }

    private sealed record TimestampPosition(DateTimeOffset Timestamp) : StartPosition
    {
        public override T Match<T>(
            Func<T> onNew,
            Func<T> onEarliest,
            Func<DateTimeOffset, T> onTimestamp,
            Func<long, T> onPosition
        ) => onTimestamp(Timestamp);
    }

    private sealed record PositionPosition(long Value) : StartPosition
    {
        public override T Match<T>(
            Func<T> onNew,
            Func<T> onEarliest,
            Func<DateTimeOffset, T> onTimestamp,
            Func<long, T> onPosition
        ) => onPosition(Value);
    }
}
