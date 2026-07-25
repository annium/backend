using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using StackExchange.Redis;
using Xunit;

namespace Annium.Redis.Tests;

/// <summary>
/// Tests for <see cref="IRedisStorage"/> connection-fault handling against an unreachable Redis
/// host. These build a standalone DI provider (rather than the shared Testcontainers-backed
/// <see cref="ServicePack"/>) so the underlying <see cref="ConnectionMultiplexer"/> connect is
/// guaranteed to fault.
/// </summary>
public class RedisStorageConnectionFaultTests
{
    /// <summary>
    /// Verifies that disposing a storage instance whose lazy connect has faulted does not throw —
    /// the <c>DisposeAsync</c> catch swallows the faulted connect.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_ConnectionFaulted_DoesNotThrow()
    {
        // arrange
        var storage = BuildUnreachableStorage();

        // trigger the lazy connect and let it fault
        await Wrap.It(async () => await storage.GetAsync(Guid.NewGuid().ToString()))
            .ThrowsAsync<RedisConnectionException>();

        // act
        var disposable = (IAsyncDisposable)storage;

        // assert
        await disposable.DisposeAsync();
    }

    /// <summary>
    /// Verifies that a connection fault is not transient state that a later call silently recovers
    /// from: once the lazy connect against an unreachable host has faulted, subsequent calls keep
    /// throwing. (This asserts fault persistence only — it does not distinguish a reused faulted
    /// task from a fresh per-call reconnect, since both surface the same fault against a genuinely
    /// unreachable socket; observing same-task reuse would require an injectable connect seam.)
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ConnectionFault_PersistsAcrossCalls_AllThrow()
    {
        // arrange
        var storage = BuildUnreachableStorage();

        // act / assert: both calls observe the connection fault
        await Wrap.It(async () => await storage.GetAsync(Guid.NewGuid().ToString()))
            .ThrowsAsync<RedisConnectionException>();
        await Wrap.It(async () => await storage.GetAsync(Guid.NewGuid().ToString()))
            .ThrowsAsync<RedisConnectionException>();
    }

    /// <summary>
    /// Builds a standalone <see cref="IRedisStorage"/> configured to point at an unreachable host
    /// (connection refused), so the lazy connect faults quickly instead of hanging.
    /// </summary>
    /// <returns>An <see cref="IRedisStorage"/> backed by an unreachable Redis endpoint.</returns>
    private static IRedisStorage BuildUnreachableStorage()
    {
        var container = new ServiceContainer();
        container.Add(new RedisConfiguration { Hosts = [new RedisHost("127.0.0.1", 1)] }).AsSelf().Singleton();
        container.AddRedis();

        var provider = container.BuildServiceProvider();

        return provider.Resolve<IRedisStorage>();
    }
}
