using System.Threading.Tasks;
using Annium.Cache.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Cache.InMemory.Tests;

/// <summary>
/// Tests for the in-memory cache implementation.
/// </summary>
public class CacheTests : CacheTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test information.</param>
    public CacheTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<ServicePack>();
    }

    /// <summary>
    /// InMemory contract: one shared instance per key — assert strict reference identity.
    /// </summary>
    /// <typeparam name="T">The reference type of the cached items.</typeparam>
    /// <param name="actual">The item under test.</param>
    /// <param name="expected">The reference item to compare against.</param>
    protected override void AssertCachedInstance<T>(T actual, T expected) => ReferenceEquals(actual, expected).IsTrue();

    /// <summary>
    /// Tests the default behavior of GetOrCreateAsync for the in-memory cache implementation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_Default()
    {
        await GetOrCreateAsync_Default_Base();
    }

    /// <summary>
    /// Tests GetOrCreateAsync with absolute expiration for the in-memory cache implementation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_AbsoluteExpiration()
    {
        await GetOrCreateAsync_AbsoluteExpiration_Base();
    }

    /// <summary>
    /// Tests GetOrCreateAsync with sliding expiration for the in-memory cache implementation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_SlidingExpiration()
    {
        await GetOrCreateAsync_SlidingExpiration_Base();
    }

    /// <summary>
    /// Tests the RemoveAsync functionality for the in-memory cache implementation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RemoveAsync()
    {
        await RemoveAsync_Base();
    }

    /// <summary>
    /// Verifies that a factory exception surfaces to the awaiting caller and the slot is unpoisoned.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_FactoryThrows_PropagatesAndUnpoisons()
    {
        await GetOrCreateAsync_FactoryThrows_PropagatesAndUnpoisons_Base();
    }

    /// <summary>
    /// Verifies that all concurrent callers observe the factory exception (no hang).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_FactoryThrows_ConcurrentCallersAllSeeException()
    {
        await GetOrCreateAsync_FactoryThrows_ConcurrentCallersAllSeeException_Base();
    }

    /// <summary>
    /// Verifies that DisposeAsync is idempotent.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        await DisposeAsync_CalledTwice_DoesNotThrow_Base();
    }

    /// <summary>
    /// Verifies that a pre-cancelled CT is observed before the factory is invoked.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_PreCancelledCt_ThrowsBeforeFactoryCall()
    {
        await GetOrCreateAsync_PreCancelledCt_ThrowsBeforeFactoryCall_Base();
    }

    /// <summary>
    /// Verifies that one caller's CT cancellation does not fault the shared TCS for other awaiters.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_CancelDuringFactory_OneAwaiterCancelsOthersContinue()
    {
        await GetOrCreateAsync_CancelDuringFactory_OneAwaiterCancelsOthersContinue_Base();
    }

    /// <summary>
    /// Verifies that RemoveAsync observes a pre-cancelled CT.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RemoveAsync_PreCancelledCt_Throws()
    {
        await RemoveAsync_PreCancelledCt_Throws_Base();
    }

    /// <summary>
    /// Verifies sliding expiration is prolonged on each hit so an accessed entry survives its original window.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_SlidingExpiration_AccessWithinWindowProlongs()
    {
        await GetOrCreateAsync_SlidingExpiration_AccessWithinWindowProlongs_Base();
    }

    /// <summary>
    /// Verifies first-writer-wins option semantics: a later caller's different options are ignored for a live key.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_FirstWriterWins_LaterCallerOptionsIgnored()
    {
        await GetOrCreateAsync_FirstWriterWins_LaterCallerOptionsIgnored_Base();
    }

    /// <summary>
    /// Verifies a factory that completes after its entry expired causes cancellation and eviction.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_FactoryCompletesAfterExpiry_CancelsAndEvicts()
    {
        await GetOrCreateAsync_FactoryCompletesAfterExpiry_CancelsAndEvicts_Base();
    }

    /// <summary>
    /// Verifies the context-carrying GetOrCreateAsync overload forwards the supplied context to the factory.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_WithContext_ContextPassedToFactory()
    {
        await GetOrCreateAsync_WithContext_ContextPassedToFactory_Base();
    }

    /// <summary>
    /// Verifies that cache operations on a disposed in-memory cache throw instead of hanging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_AfterDispose_Throws()
    {
        await GetOrCreateAsync_AfterDispose_Throws_Base();
    }

    /// <summary>
    /// Verifies that disposing the in-memory cache while a factory is in-flight drains cleanly.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_WhileFactoryInFlight_DrainsAndCallerCompletes()
    {
        await DisposeAsync_WhileFactoryInFlight_DrainsAndCallerCompletes_Base();
    }

    /// <summary>
    /// Verifies concurrent access across many distinct keys runs each key's factory exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_ConcurrentDistinctKeys_EachFactoryOnce()
    {
        await GetOrCreateAsync_ConcurrentDistinctKeys_EachFactoryOnce_Base();
    }

    /// <summary>
    /// Verifies that removing a non-existent key from the in-memory cache is a silent no-op.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RemoveAsync_NonExistentKey_CompletesWithoutException()
    {
        await RemoveAsync_NonExistentKey_CompletesWithoutException_Base();
    }

    /// <summary>
    /// Verifies an expired in-flight factory does not evict a concurrently-created replacement entry.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_ExpiredFactoryAfterReplacement_ReplacementSurvives()
    {
        await GetOrCreateAsync_ExpiredFactoryAfterReplacement_ReplacementSurvives_Base();
    }

    /// <summary>
    /// Verifies a throwing in-flight factory does not evict a concurrently-created replacement entry.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_ThrowingFactoryAfterReplacement_ReplacementSurvives()
    {
        await GetOrCreateAsync_ThrowingFactoryAfterReplacement_ReplacementSurvives_Base();
    }

    /// <summary>
    /// Verifies RemoveAsync during an in-flight factory still delivers the value and purges the entry.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RemoveAsync_WhileFactoryInFlight_CallerGetsValueThenReinvokes()
    {
        await RemoveAsync_WhileFactoryInFlight_CallerGetsValueThenReinvokes_Base();
    }
}
