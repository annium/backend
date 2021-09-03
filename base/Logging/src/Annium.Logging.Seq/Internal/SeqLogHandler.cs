using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Collections.Generic;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using NodaTime;

namespace Annium.Logging.Seq.Internal
{
    internal class SeqLogHandler : IAsyncLogHandler
    {
        private static readonly DateTimeZone Tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
        private readonly Queue<IReadOnlyDictionary<string, string>> _eventsBuffer = new();
        private readonly IHttpRequest _request;
        private readonly ISerializer<string> _serializer;
        private readonly string _project;
        private readonly SeqConfiguration _cfg;

        public SeqLogHandler(
            IHttpRequestFactory httpRequestFactory,
            ISerializer<string> serializer,
            string project,
            SeqConfiguration cfg
        )
        {
            _request = httpRequestFactory.New(cfg.Endpoint);
            _serializer = serializer;
            _project = project;
            _cfg = cfg;
        }

        public async ValueTask Handle(IReadOnlyCollection<LogMessage> messages)
        {
            var events = new List<IReadOnlyDictionary<string, string>>();

            void AddEvent(LogMessage msg, string message) => events.Add(CompactLogEvent.Format(_project, msg, message, Tz));
            foreach (var message in messages)
                Process(message, AddEvent);

            // if failed to send events - add them to buffer
            if (!await SendEventsAsync(events))
            {
                BufferEvents(events);
                return;
            }

            while (true)
            {
                // pick slice to send
                lock (_eventsBuffer)
                {
                    if (_eventsBuffer.Count == 0)
                        break;

                    events.Clear();

                    var count = Math.Min(_eventsBuffer.Count, _cfg.BufferCount);
                    for (var i = 0; i < count; i++)
                        events.Add(_eventsBuffer.Dequeue());
                }

                // if sent successfully - go to next slice
                if (await SendEventsAsync(events))
                    continue;

                // if failed to send - move events back to buffer and break sending
                BufferEvents(events);
                break;
            }
        }

        private async ValueTask<bool> SendEventsAsync(IReadOnlyCollection<IReadOnlyDictionary<string, string>> events)
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

        private void BufferEvents(IReadOnlyCollection<IReadOnlyDictionary<string, string>> events)
        {
            lock (_eventsBuffer)
                foreach (var e in events)
                    _eventsBuffer.Enqueue(e);
        }
    }
}