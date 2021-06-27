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
        private static readonly DateTimeZone Tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
        private readonly IHttpRequest _request;

        public SeqLogHandler(
            IHttpRequestFactory httpRequestFactory,
            ISerializer<string> serializer,
            Uri endpoint
        )
        {
            _serializer = serializer;
            _request = httpRequestFactory.New(endpoint);
        }

        public async ValueTask Handle(IReadOnlyCollection<LogMessage> messages)
        {
            var events = new List<IReadOnlyDictionary<string, string>>();
            void AddEvent(LogMessage msg, string message) => events.Add(CompactLogEvent.Format(msg, message, Tz));
            foreach (var message in messages)
                Process(message, AddEvent);

            var data = events.Select(_serializer.Serialize).Join(Environment.NewLine);

            var response = await _request.Clone()
                .Post("api/events/raw")
                .Param("clef", string.Empty)
                .StringContent(data, "application/vnd.serilog.clef")
                .RunAsync();
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