using System;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;

namespace Annium.AspNetCore.IntegrationTesting
{
    public interface IWebApplicationFactory
    {
        TestServer Server { get; }
        IServiceProvider ServiceProvider { get; }
        HttpClient CreateClient();
    }
}