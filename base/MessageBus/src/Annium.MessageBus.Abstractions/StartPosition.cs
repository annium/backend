using System;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Where a replay-capable subscription starts consuming from. Closed union — construct via the factory members
/// and consume via <see cref="Match{T}"/> / <see cref="Switch"/> (concrete cases are intentionally private).
/// Only meaningful for transports implementing <see cref="IReplayableMessageSubscriber"/>.
/// </summary>
public abstract record StartPosition
{
    /// <summary>
    /// Prevents external derivation — the union is closed over the private cases declared below.
    /// </summary>
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

    /// <summary>
    /// The "new messages only" case: consumes only messages produced after subscription (no history).
    /// </summary>
    private sealed record NewPosition : StartPosition
    {
        /// <summary>
        /// Invokes <paramref name="onNew"/> for this case and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="onNew">Called for the "new messages only" case.</param>
        /// <param name="onEarliest">Unused for this case.</param>
        /// <param name="onTimestamp">Unused for this case.</param>
        /// <param name="onPosition">Unused for this case.</param>
        /// <returns>The value produced by <paramref name="onNew"/>.</returns>
        public override T Match<T>(
            Func<T> onNew,
            Func<T> onEarliest,
            Func<DateTimeOffset, T> onTimestamp,
            Func<long, T> onPosition
        ) => onNew();
    }

    /// <summary>
    /// The "earliest retained" case: consumes from the earliest retained message.
    /// </summary>
    private sealed record EarliestPosition : StartPosition
    {
        /// <summary>
        /// Invokes <paramref name="onEarliest"/> for this case and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="onNew">Unused for this case.</param>
        /// <param name="onEarliest">Called for the "earliest retained" case.</param>
        /// <param name="onTimestamp">Unused for this case.</param>
        /// <param name="onPosition">Unused for this case.</param>
        /// <returns>The value produced by <paramref name="onEarliest"/>.</returns>
        public override T Match<T>(
            Func<T> onNew,
            Func<T> onEarliest,
            Func<DateTimeOffset, T> onTimestamp,
            Func<long, T> onPosition
        ) => onEarliest();
    }

    /// <summary>
    /// The timestamp-based case: consumes from the first message at or after a given timestamp.
    /// </summary>
    /// <param name="Timestamp">The moment to start consuming from; the first message at or after it.</param>
    private sealed record TimestampPosition(DateTimeOffset Timestamp) : StartPosition
    {
        /// <summary>
        /// Invokes <paramref name="onTimestamp"/> with this case's timestamp and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="onNew">Unused for this case.</param>
        /// <param name="onEarliest">Unused for this case.</param>
        /// <param name="onTimestamp">Called for the timestamp case, with its timestamp.</param>
        /// <param name="onPosition">Unused for this case.</param>
        /// <returns>The value produced by <paramref name="onTimestamp"/>.</returns>
        public override T Match<T>(
            Func<T> onNew,
            Func<T> onEarliest,
            Func<DateTimeOffset, T> onTimestamp,
            Func<long, T> onPosition
        ) => onTimestamp(Timestamp);
    }

    /// <summary>
    /// The position-based case: consumes from a given transport sequence/offset.
    /// </summary>
    /// <param name="Value">The transport sequence / offset to start consuming from.</param>
    private sealed record PositionPosition(long Value) : StartPosition
    {
        /// <summary>
        /// Invokes <paramref name="onPosition"/> with this case's value and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="onNew">Unused for this case.</param>
        /// <param name="onEarliest">Unused for this case.</param>
        /// <param name="onTimestamp">Unused for this case.</param>
        /// <param name="onPosition">Called for the sequence/offset case, with its value.</param>
        /// <returns>The value produced by <paramref name="onPosition"/>.</returns>
        public override T Match<T>(
            Func<T> onNew,
            Func<T> onEarliest,
            Func<DateTimeOffset, T> onTimestamp,
            Func<long, T> onPosition
        ) => onPosition(Value);
    }
}
