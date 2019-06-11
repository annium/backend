using System;
using Annium.Extensions.Net.Http;

namespace Annium.AspNetCore.IntegrationTesting
{
    public class IntegrationTest<TStartup> where TStartup : class
    {
        protected IRequest http => configureRequest(getRequest()).Clone();

        private readonly Func<IRequest> getRequest;

        private Func<IRequest, IRequest> configureRequest = r => r;

        public IntegrationTest()
        {
            var client = new TestWebApplicationFactory<TStartup>().CreateClient();
            getRequest = () => Http.Open().UseClient(client);
        }

        protected void Configure(Func<IRequest, IRequest> configureRequest) =>
            this.configureRequest = configureRequest;
    }
}