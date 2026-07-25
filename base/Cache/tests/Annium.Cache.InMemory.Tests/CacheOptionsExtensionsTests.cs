using System;
using Annium.Cache.Abstractions;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Cache.InMemory.Tests;

/// <summary>
/// Unit tests for <see cref="CacheOptionsExtensions.GetExpiresAt"/> expiration calculation across modes,
/// plus the <see cref="CacheOptions"/> factory validation guards.
/// </summary>
public class CacheOptionsExtensionsTests
{
    /// <summary>
    /// Verifies sliding expiration rejects a non-positive lifetime (which would yield an immediately-expired entry).
    /// </summary>
    [Fact]
    public void WithSlidingExpiration_NonPositiveLifetime_Throws()
    {
        Wrap.It(() => CacheOptions.WithSlidingExpiration(Duration.Zero)).Throws<ArgumentOutOfRangeException>();
        Wrap.It(() => CacheOptions.WithSlidingExpiration(Duration.FromSeconds(-1)))
            .Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies absolute expiration rejects the <see cref="Instant.MinValue"/> sentinel (a permanently-expired moment).
    /// </summary>
    [Fact]
    public void WithAbsoluteExpiration_MinValueMoment_Throws()
    {
        Wrap.It(() => CacheOptions.WithAbsoluteExpiration(Instant.MinValue)).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies absolute-mode options return the configured moment regardless of the supplied current time.
    /// </summary>
    [Fact]
    public void GetExpiresAt_AbsoluteMode_ReturnsMoment()
    {
        var moment = Instant.FromUtc(2030, 1, 1, 0, 0);
        var options = CacheOptions.WithAbsoluteExpiration(moment);

        options.GetExpiresAt(Instant.FromUtc(2020, 6, 1, 0, 0)).Is(moment);
    }

    /// <summary>
    /// Verifies sliding-mode options return the current time plus the configured lifetime.
    /// </summary>
    [Fact]
    public void GetExpiresAt_SlidingMode_ReturnsNowPlusLifetime()
    {
        var lifetime = Duration.FromMinutes(5);
        var options = CacheOptions.WithSlidingExpiration(lifetime);
        var now = Instant.FromUtc(2025, 3, 15, 12, 0);

        options.GetExpiresAt(now).Is(now + lifetime);
    }
}
