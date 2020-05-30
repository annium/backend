using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution
{
    public class BatchExecutor
    {
        private IList<Delegate> handlers = new List<Delegate>();

        internal BatchExecutor()
        {
        }

        public BatchExecutor With(Action handler) => WithInternal(handler);

        public BatchExecutor With(Func<Task> handler) => WithInternal(handler);

        private BatchExecutor WithInternal(Delegate handler)
        {
            handlers.Add(handler);

            return this;
        }

        public async Task RunAsync()
        {
            var exceptions = new List<Exception>();

            // run each stage
            foreach (var handler in handlers)
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