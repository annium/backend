using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Pins the lazy logger resolution contract of <see cref="TestHostBase{TEntryPoint}" />: the logger is
/// resolved from the started application's own service provider and cached across accesses, and
/// accessing it (or any other member backed by <c>AppFactory</c>) before
/// <see cref="TestHostBase{TEntryPoint}.StartAsync" /> throws.
/// </summary>
public class LazyLoggerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the LazyLoggerTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public LazyLoggerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that <see cref="TestHostBase{TEntryPoint}.Logger" /> resolves a non-null logger from the
    /// started application, and returns the same cached instance on repeated access, pinning the
    /// lazy-caching behavior of the underlying <see cref="Lazy{T}" />.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Logger_AfterStart_ResolvesAndCachesLoggerInstance()
    {
        // arrange
        await using var testHost = new TestHost(OutputHelper);
        await testHost.StartAsync();

        // act
        var first = testHost.Logger;
        var second = testHost.Logger;

        // assert
        first.IsNotNull();
        second.Is(first);
    }

    /// <summary>
    /// Tests that accessing <see cref="TestHostBase{TEntryPoint}.Logger" /> before the host has been
    /// started throws, since the backing application factory has not been created yet.
    /// </summary>
    [Fact]
    public void Logger_HostNotStarted_ThrowsInvalidOperationException()
    {
        // arrange
        // The host is never started, so no underlying application factory is ever created - nothing to
        // dispose, so it is intentionally left unstarted and undisposed.
        var testHost = new TestHost(OutputHelper);

        // act & assert
        Wrap.It(() => _ = testHost.Logger).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that disposing a <see cref="TestHostBase{TEntryPoint}" /> that was never started throws, since
    /// <see cref="TestHostBase{TEntryPoint}.DisposeAsync" /> reads the backing application factory, whose
    /// getter throws when no application factory has been created yet. This pins today's actual behavior
    /// as a characterization test, not an endorsement of it.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_HostNotStarted_ThrowsInvalidOperationException()
    {
        // arrange
        // The host is never started, so DisposeAsync itself is expected to throw - nothing else to dispose.
        var testHost = new TestHost(OutputHelper);

        // act & assert
        await Wrap.It(() => testHost.DisposeAsync()).ThrowsAsync<InvalidOperationException>();
    }
}
