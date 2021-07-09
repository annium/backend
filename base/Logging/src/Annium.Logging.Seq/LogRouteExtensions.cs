using Annium.Logging.Seq;
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
            string project,
            SeqConfiguration configuration
        )
        {
            route.UseAsyncFactory(sp =>
            {
                var httpRequestFactory = sp.Resolve<IHttpRequestFactory>();
                var serializer = sp.Resolve<ISerializer<string>>();
                return new SeqLogHandler(httpRequestFactory, serializer, project, configuration);
            }, configuration);

            return route;
        }
    }
}