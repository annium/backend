using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution.Internal
{
    internal class BatchExecutor : IBatchExecutor
    {
        private readonly IList<Delegate> _handlers = new List<Delegate>();

        public IBatchExecutor With(Action handler) => WithInternal(handler);

        public IBatchExecutor With(Func<Task> handler) => WithInternal(handler);

        private BatchExecutor WithInternal(Delegate handler)
        {
            _handlers.Add(handler);

            return this;
        }

        public async Task RunAsync()
        {
            var exceptions = new List<Exception>();

            // run each stage
            foreach (var handler in _handlers)
            {
                try
                {
                    if (handler is Func<Task> handleAsync) await handleAsync();
                    if (handler is Action handleSync) handleSync();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }
    }
}