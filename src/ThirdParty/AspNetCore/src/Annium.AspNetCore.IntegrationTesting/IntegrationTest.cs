using System;
using System.Collections.Concurrent;
using Annium.Extensions.Net.Http;

namespace Annium.AspNetCore.IntegrationTesting
{
    public class IntegrationTest
    {
        private ConcurrentDictionary<Type, IRequest> requestSamples = new ConcurrentDictionary<Type, IRequest>();

        protected IRequest GetRequest<TStartup>(Func<IRequest, IRequest> configureRequest = null)
        where TStartup : class
        {
            var request = requestSamples.GetOrAdd(typeof(TStartup), _ =>
            {
                var client = new TestWebApplicationFactory<TStartup>().CreateClient();

                return Http.Open().UseClient(client);
            }).Clone();

            return configureRequest == null ? request : configureRequest(request);
        }
    }
}