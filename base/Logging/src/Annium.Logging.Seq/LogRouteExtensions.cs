using System;
using Annium.Logging.Seq.Internal;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseSeq(
            this LogRoute route,
            Uri endpoint
        ) => route.UseSeq(endpoint, new LogRouteConfiguration(TimeSpan.FromSeconds(5), 100));

        public static LogRoute UseSeq(
            this LogRoute route,
            Uri endpoint,
            LogRouteConfiguration configuration
        )
        {
            route.UseAsyncFactory(sp =>
            {
                var httpRequestFactory = sp.Resolve<IHttpRequestFactory>();
                var serializer = sp.Resolve<ISerializer<string>>();
                return new SeqLogHandler(httpRequestFactory, serializer, endpoint);
            }, configuration);

            return route;
        }
    }
}