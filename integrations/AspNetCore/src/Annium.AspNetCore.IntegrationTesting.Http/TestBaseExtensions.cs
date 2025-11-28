using Annium.Net.Http;
using Annium.Testing;

namespace Annium.AspNetCore.IntegrationTesting.Http;

public static class TestBaseExtensions
{
    public static void RegisterHttpRequestFactory(this TestBase test, ITestHost testHost, bool isDefault = false)
    {
        test.Register(container =>
        {
            container.AddHttpRequestFactory(_ => testHost.Server.CreateClient(), isDefault);
        });
    }

    public static void RegisterHttpRequestFactory(
        this TestBase test,
        string key,
        ITestHost testHost,
        bool isDefault = false
    )
    {
        test.Register(container =>
        {
            container.AddHttpRequestFactory(key, (_, _) => testHost.Server.CreateClient(), isDefault);
        });
    }
}
