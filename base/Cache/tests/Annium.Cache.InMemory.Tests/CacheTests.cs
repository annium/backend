using System.Threading.Tasks;
using Annium.Cache.Tests.Lib;
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
}
