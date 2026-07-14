using Amazon;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.S3.Tests;

/// <summary>
/// Tests for how the S3 client is pointed at a server, covering the AWS endpoint the container-backed
/// suites never reach.
/// </summary>
public class ClientConfigTest
{
    /// <summary>
    /// Tests that a configured server is addressed directly, with the region kept as the signing
    /// region rather than as the endpoint.
    /// </summary>
    [Fact]
    public void GetClientConfig_Server_TargetsServer()
    {
        // arrange
        var configuration = GetConfiguration() with
        {
            Server = "http://localhost:9000",
        };

        // act
        var config = Internal.Storage.GetClientConfig(configuration);

        // assert: the SDK normalizes the endpoint with a trailing slash
        config.ServiceURL.Is("http://localhost:9000/");
        config.AuthenticationRegion.Is("us-east-1");
    }

    /// <summary>
    /// Tests that with no server configured the client is left to address AWS itself by region —
    /// the path a live cloud account uses, which no test server stands in for.
    /// </summary>
    [Fact]
    public void GetClientConfig_NoServer_TargetsAwsRegion()
    {
        // arrange
        var configuration = GetConfiguration();

        // act
        var config = Internal.Storage.GetClientConfig(configuration);

        // assert: no endpoint of our own, leaving the SDK to address AWS by region
        config.ServiceURL.IsDefault();
        config.RegionEndpoint.Is(RegionEndpoint.GetBySystemName("us-east-1"));
    }

    /// <summary>
    /// Creates a configuration that passes the guards, leaving the server unset.
    /// </summary>
    /// <returns>A valid configuration with no server.</returns>
    private static Configuration GetConfiguration() =>
        new()
        {
            AccessKey = "access-key",
            AccessSecret = "access-secret",
            Region = "us-east-1",
            Bucket = "bucket",
        };
}
