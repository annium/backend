using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Annium.AspNetCore.IntegrationTesting.Internal
{
    internal class WebApplicationFactoryWrapper<TStartup> : IWebApplicationFactory
        where TStartup : class
    {
        public TestServer Server => _appFactory.Server;
        public IServiceProvider ServiceProvider => _appFactory.Services;

        private readonly WebApplicationFactory<TStartup> _appFactory;

        public WebApplicationFactoryWrapper(
            WebApplicationFactory<TStartup> appFactory
        )
        {
            _appFactory = appFactory;
        }

        public HttpClient CreateClient() => _appFactory.CreateClient();
    }
}