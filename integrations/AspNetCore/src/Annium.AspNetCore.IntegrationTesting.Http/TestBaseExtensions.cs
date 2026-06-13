using System;
using Annium.Net.Http;
using Annium.Testing;

namespace Annium.AspNetCore.IntegrationTesting.Http;

public static class TestBaseExtensions
{
    // The test host is created inside the test body, but TestBase freezes its registrations once
    // InitializeAsync has begun (before the body runs). So the factory is registered in the test
    // constructor against a deferred host accessor; the delegate is invoked at resolve time, by
    // which point the body has started the host and populated the accessor.
    public static void RegisterHttpRequestFactory(this TestBase test, Func<ITestHost> testHost, bool isDefault = false)
    {
        test.Register(container =>
        {
            container.AddHttpRequestFactory(_ => testHost().Server.CreateClient(), isDefault);
        });
    }

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
