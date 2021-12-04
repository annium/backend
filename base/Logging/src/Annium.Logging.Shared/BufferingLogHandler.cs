using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Logging.Shared;

public abstract class BufferingLogHandler<TContext, TMessage> : IAsyncLogHandler<TContext>
    where TContext : class, ILogContext
{
    private readonly Func<LogMessage<TContext>, string, TMessage> _format;
    private readonly LogRouteConfiguration _cfg;
    private readonly Queue<TMessage> _eventsBuffer = new();

    protected BufferingLogHandler(
        Func<LogMessage<TContext>, string, TMessage> format,
        LogRouteConfiguration cfg
    )
    {
        _format = format;
        _cfg = cfg;
    }

    public async ValueTask Handle(IReadOnlyCollection<LogMessage<TContext>> messages)
    {
        var events = new List<TMessage>();

        void AddEvent(LogMessage<TContext> msg, string message) => events
            .Add(_format(msg, message));

        foreach (var message in messages)
            LogMessageProcessor.Process(message, AddEvent);

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

    protected abstract ValueTask<bool> SendEventsAsync(IReadOnlyCollection<TMessage> events);

    private void BufferEvents(IReadOnlyCollection<TMessage> events)
    {
        lock (_eventsBuffer)
            foreach (var e in events)
                _eventsBuffer.Enqueue(e);
    }
}