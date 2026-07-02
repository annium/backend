using System;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for <see cref="StartPosition"/> factories and equality.
/// </summary>
public class StartPositionTests
{
    /// <summary>
    /// The <see cref="StartPosition.New"/> and <see cref="StartPosition.Earliest"/> members are stable singletons.
    /// </summary>
    [Fact]
    public void New_And_Earliest_AreSingletons()
    {
        StartPosition.New.Is(StartPosition.New);
        StartPosition.Earliest.Is(StartPosition.Earliest);
        (StartPosition.New == StartPosition.Earliest).Is(false);
    }

    /// <summary>
    /// Timestamp positions compare by value.
    /// </summary>
    [Fact]
    public void FromTimestamp_EqualsByValue()
    {
        var t = new DateTimeOffset(2026, 7, 2, 10, 0, 0, TimeSpan.Zero);
        StartPosition.FromTimestamp(t).Is(StartPosition.FromTimestamp(t));
        (StartPosition.FromTimestamp(t) == StartPosition.FromTimestamp(t.AddSeconds(1))).Is(false);
    }

    /// <summary>
    /// Position (sequence/offset) positions compare by value.
    /// </summary>
    [Fact]
    public void FromPosition_EqualsByValue()
    {
        StartPosition.FromPosition(42).Is(StartPosition.FromPosition(42));
        (StartPosition.FromPosition(42) == StartPosition.FromPosition(43)).Is(false);
    }

    /// <summary>
    /// Different variants are never equal.
    /// </summary>
    [Fact]
    public void DifferentVariants_AreNotEqual()
    {
        var t = DateTimeOffset.UnixEpoch;
        (StartPosition.New == StartPosition.FromTimestamp(t)).Is(false);
        (StartPosition.FromTimestamp(t) == StartPosition.FromPosition(0)).Is(false);
    }

    /// <summary>
    /// <see cref="StartPosition.Match{T}"/> dispatches to the matching case and surfaces its data.
    /// </summary>
    [Fact]
    public void Match_DispatchesByCase()
    {
        static string Describe(StartPosition p) =>
            p.Match(
                onNew: () => "new",
                onEarliest: () => "earliest",
                onTimestamp: t => $"ts:{t:O}",
                onPosition: v => $"pos:{v}"
            );

        var t = new DateTimeOffset(2026, 7, 2, 0, 0, 0, TimeSpan.Zero);
        Describe(StartPosition.New).Is("new");
        Describe(StartPosition.Earliest).Is("earliest");
        Describe(StartPosition.FromPosition(7)).Is("pos:7");
        Describe(StartPosition.FromTimestamp(t)).Is($"ts:{t:O}");
    }
}
