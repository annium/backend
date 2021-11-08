using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Primitives.Collections.Generic;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Logging.Seq.Internal
{
    internal class SeqLogHandler<TContext> : BufferingLogHandler<TContext, IReadOnlyDictionary<string, string>>
        where TContext : class, ILogContext
    {
        private readonly IHttpRequest _request;
        private readonly ISerializer<string> _serializer;
        private readonly SeqConfiguration _cfg;

        public SeqLogHandler(
            IHttpRequestFactory httpRequestFactory,
            ISerializer<string> serializer,
            SeqConfiguration cfg
        ) : base(
            CompactLogEvent<TContext>.CreateFormat(cfg.Project),
            cfg
        )
        {
            _request = httpRequestFactory.New(cfg.Endpoint);
            _serializer = serializer;
            _cfg = cfg;
        }

        protected override async ValueTask<bool> SendEventsAsync(
            IReadOnlyCollection<IReadOnlyDictionary<string, string>> events
        )
        {
            var data = events.Select(_serializer.Serialize).Join(Environment.NewLine);

            try
            {
                var response = await _request.Clone()
                    .Post("api/events/raw")
                    .Param("clef", string.Empty)
                    .Header("X-Seq-ApiKey", _cfg.ApiKey)
                    .StringContent(data, "application/vnd.serilog.clef")
                    .RunAsync();
                if (response.IsSuccess)
                    return true;

                Console.WriteLine($"Failed to to write events to Seq at {_request.Uri}: {response.StatusCode} - {response.StatusText}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to to write events to Seq at {_request.Uri}: {e}");
                return false;
            }
        }
    }
}