using Annium.AspNetCore.TestServer;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Base class for integration tests that provides a configured web application factory
/// </summary>
public abstract class IntegrationTestBase : IntegrationTest
{
    /// <summary>
    /// Gets the web application factory for testing
    /// </summary>
    protected IWebApplicationFactory AppFactory { get; }

    /// <summary>
    /// Initializes a new instance of the IntegrationTestBase class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    protected IntegrationTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AppFactory = GetAppFactory<Program>(builder => builder.UseServicePack<TestServicePack>());
    }
}
