using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Storage.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.FileSystem.Tests;

/// <summary>
/// Tests for the file system configuration guards, which reject a malformed root directory
/// on construction, before any storage operation is attempted.
/// </summary>
public class ConfigurationTest
{
    /// <summary>
    /// Tests that a root directory that is not absolute is rejected.
    /// </summary>
    [Fact]
    public void RelativeDirectory_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => GetStorage("files")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a root directory with a trailing slash is rejected.
    /// </summary>
    [Fact]
    public void TrailingSlashDirectory_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => GetStorage("/files/")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a root directory with a malformed segment is rejected.
    /// </summary>
    [Fact]
    public void InvalidDirectoryPart_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => GetStorage("/files/..")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Creates a file system storage instance rooted at the given directory.
    /// </summary>
    /// <param name="directory">The root directory under test.</param>
    /// <returns>A configured file system storage instance.</returns>
    private static IStorage GetStorage(string directory)
    {
        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        services.AddFileSystemStorage("default", (_, _) => new Configuration { Directory = directory }, true);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }
}
