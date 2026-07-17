using System.Threading.Tasks;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Pins the scope-resolution contract of <see cref="TestHostBase{TEntryPoint}.CreateAsyncScope" />: it creates
/// a scope backed by the hosted application's own service provider, resolving the same scoped instance for
/// repeated resolutions within one scope, and distinct instances across two separate scopes.
/// </summary>
public class ScopedResolutionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ScopedResolutionTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public ScopedResolutionTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that <see cref="TestHostBase{TEntryPoint}.CreateAsyncScope" /> resolves a scoped service registered
    /// in the hosted application, returning the same instance for two resolutions within one scope, and a
    /// distinct instance from a second, independent scope.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task CreateAsyncScope_ResolvesScopedServiceFromHostedApplication()
    {
        // arrange
        await using var testHost = await new ScopedTestHost(OutputHelper).StartAsync();

        // act
        await using var scopeOne = testHost.CreateAsyncScope();
        var scopeOneFirst = scopeOne.ServiceProvider.GetRequiredService<IScopedMarker>();
        var scopeOneSecond = scopeOne.ServiceProvider.GetRequiredService<IScopedMarker>();

        await using var scopeTwo = testHost.CreateAsyncScope();
        var scopeTwoFirst = scopeTwo.ServiceProvider.GetRequiredService<IScopedMarker>();

        // assert
        scopeOneFirst.IsNotNull();
        scopeOneSecond.Is(scopeOneFirst);
        scopeTwoFirst.IsNotNull();
        scopeTwoFirst.Id.IsNotEqual(scopeOneFirst.Id);
    }
}
