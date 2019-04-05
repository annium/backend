using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution
{
    public class StageExecutor
    {
        private IList<ValueTuple<Delegate, Delegate, bool>> stages =
            new List<ValueTuple<Delegate, Delegate, bool>>();

        internal StageExecutor() { }

        public StageExecutor Stage(Action commit, Action rollback, bool rollbackFailed = false) =>
            StageInternal(commit, rollback, rollbackFailed);

        public StageExecutor Stage(Action commit, Func<Task> rollback, bool rollbackFailed = false) =>
            StageInternal(commit, rollback, rollbackFailed);

        public StageExecutor Stage(Func<Task> commit, Action rollback, bool rollbackFailed = false) =>
            StageInternal(commit, rollback, rollbackFailed);

        public StageExecutor Stage(Func<Task> commit, Func<Task> rollback, bool rollbackFailed = false) =>
            StageInternal(commit, rollback, rollbackFailed);

        private StageExecutor StageInternal(Delegate commit, Delegate rollback, bool rollbackFailed)
        {
            stages.Add((commit, rollback, rollbackFailed));

            return this;
        }

        public async Task RunAsync()
        {
            var i = 0;
            var exceptions = new List<Exception>();;

            // run each stage
            foreach (var(commit, _, _) in stages)
            {
                try
                {
                    // count before stage run to include failed stage
                    i++;

                    if (commit is Func<Task> commitAsync) await commitAsync();
                    if (commit is Action commitSync) commitSync();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            // if no exceptions - done
            if (exceptions.Count == 0)
                return;

            var j = 0;
            // exception caught, rollback
            foreach (var(_, rollback, rollbackFailed) in stages.Take(i))
            {
                try
                {
                    // if current stage is failed and is not wanted to be rolled back - break (it's last step by design)
                    if (j++ == i && !rollbackFailed)
                        break;

                    if (rollback is Func<Task> rollbackAsync) await rollbackAsync();
                    if (rollback is Action rollbackSync) rollbackSync();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}