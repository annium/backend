using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using NodaTime;

namespace Annium.Logging.Seq.Internal
{
    internal class SeqLogHandler : IAsyncLogHandler
    {
        private readonly ISerializer<string> _serializer;
        private readonly string _project;
        private static readonly DateTimeZone Tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
        private readonly IHttpRequest _request;
        private readonly string _apiKey;

        public SeqLogHandler(
            IHttpRequestFactory httpRequestFactory,
            ISerializer<string> serializer,
            string project,
            SeqConfiguration cfg
        )
        {
            _serializer = serializer;
            _project = project;
            _request = httpRequestFactory.New(cfg.Endpoint);
            _apiKey = cfg.ApiKey;
        }

        public async ValueTask Handle(IReadOnlyCollection<LogMessage> messages)
        {
            var events = new List<IReadOnlyDictionary<string, string>>();
            void AddEvent(LogMessage msg, string message) => events.Add(CompactLogEvent.Format(_project, msg, message, Tz));
            foreach (var message in messages)
                Process(message, AddEvent);

            var data = events.Select(_serializer.Serialize).Join(Environment.NewLine);

            var response = await _request.Clone()
                .Post("api/events/raw")
                .Param("clef", string.Empty)
                .Header("X-Seq-ApiKey", _apiKey)
                .StringContent(data, "application/vnd.serilog.clef")
                .RunAsync();
            if (response.IsFailure)
                Console.WriteLine($"Failed to to write events to Seq at {_request.Uri}: {response.StatusCode} - {response.StatusText}");
        }

        private void Process(LogMessage msg, Action<LogMessage, string> addEvent)
        {
            if (msg.Exception is AggregateException aggregateException)
            {
                var errors = aggregateException.Flatten().InnerExceptions;
                addEvent(msg, $"{errors.Count} error(s) in: {GetExceptionMessage(aggregateException)}");

                foreach (var error in errors)
                    addEvent(msg, GetExceptionMessage(error));
            }
            else if (msg.Exception != null)
                addEvent(msg, GetExceptionMessage(msg.Exception));
            else
                addEvent(msg, msg.Message);
        }

        private string GetExceptionMessage(Exception e) => $"{e.Message}{e.StackTrace}";
    }
}