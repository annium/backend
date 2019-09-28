using System;
using System.Collections.Concurrent;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting
{
    public class IntegrationTest
    {
        private readonly Action<IHostBuilder> configureHost;
        private readonly ConcurrentDictionary<Type, IRequest> requestSamples = new ConcurrentDictionary<Type, IRequest>();

        public IntegrationTest(Action<IHostBuilder> configureHost)
        {
            this.configureHost = configureHost;
        }

        public IntegrationTest(Action<IServiceProviderBuilder> configureContainer)
        {
            var providerFactory = new ServiceProviderFactory(configureContainer);

            configureHost = builder =>
            {
                builder.UseServiceProviderFactory(providerFactory);
            };
        }

        protected IRequest GetRequest<TStartup>() where TStartup : class =>
            GetRequestBase<TStartup>();

        protected IRequest GetRequest<TStartup>(Func<IRequest, IRequest> configureRequest) where TStartup : class =>
            configureRequest(GetRequestBase<TStartup>());

        private IRequest GetRequestBase<TStartup>() where TStartup : class =>
            requestSamples.GetOrAdd(typeof(TStartup), _ =>
            {
                var client = new TestWebApplicationFactory<TStartup>(configureHost).CreateClient();

                return Http.Open().UseClient(client);
            }).Clone();
    }
}