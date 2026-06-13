using System;
using Annium.Net.Http;
using Annium.Testing;

namespace Annium.AspNetCore.IntegrationTesting.Http;

/// <summary>
/// Extension methods on <see cref="TestBase" /> for registering HTTP request factories backed by an
/// <see cref="ITestHost" />.
/// </summary>
public static class TestBaseExtensions
{
    // The test host is created inside the test body, but TestBase freezes its registrations once
    // InitializeAsync has begun (before the body runs). So the factory is registered in the test
    // constructor against a deferred host accessor; the delegate is invoked at resolve time, by
    // which point the body has started the host and populated the accessor.

    /// <summary>
    /// Registers an HTTP request factory on the test container that creates <see cref="System.Net.Http.HttpClient" />
    /// instances against the test server exposed by the deferred <paramref name="testHost" /> accessor.
    /// The factory delegate is resolved lazily — invoked only after the container is built — so it is
    /// safe to pass an accessor that is populated later in the test body, before the container freezes.
    /// </summary>
    /// <param name="test">The <see cref="TestBase" /> instance whose container receives the factory registration.</param>
    /// <param name="testHost">A delegate that returns the <see cref="ITestHost" /> whose server will be used to create HTTP clients.</param>
    /// <param name="isDefault">When <c>true</c>, registers this factory as the default HTTP request factory in the container.</param>
    public static void RegisterHttpRequestFactory(this TestBase test, Func<ITestHost> testHost, bool isDefault = false)
    {
        test.Register(container =>
        {
            container.AddHttpRequestFactory(_ => testHost().Server.CreateClient(), isDefault);
        });
    }

    /// <summary>
    /// Registers a keyed HTTP request factory on the test container that creates <see cref="System.Net.Http.HttpClient" />
    /// instances against the test server exposed by the deferred <paramref name="testHost" /> accessor.
    /// The factory delegate is resolved lazily — invoked only after the container is built — so it is
    /// safe to pass an accessor that is populated later in the test body, before the container freezes.
    /// </summary>
    /// <param name="test">The <see cref="TestBase" /> instance whose container receives the factory registration.</param>
    /// <param name="key">The string key under which this HTTP request factory is registered in the container.</param>
    /// <param name="testHost">A delegate that returns the <see cref="ITestHost" /> whose server will be used to create HTTP clients.</param>
    /// <param name="isDefault">When <c>true</c>, registers this factory as the default HTTP request factory in the container.</param>
    public static void RegisterHttpRequestFactory(
        this TestBase test,
        string key,
        Func<ITestHost> testHost,
        bool isDefault = false
    )
    {
        test.Register(container =>
        {
            container.AddHttpRequestFactory(key, (_, _) => testHost().Server.CreateClient(), isDefault);
        });
    }
}
