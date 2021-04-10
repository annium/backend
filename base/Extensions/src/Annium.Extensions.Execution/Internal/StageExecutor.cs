using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution.Internal
{
    internal class StageExecutor : IStageExecutor
    {
        private readonly List<ValueTuple<Delegate, Delegate, bool>> _stages = new();

        public IStageExecutor Stage(Action commit, Action rollback, bool rollbackFailed = false) =>
            StageInternal(commit, rollback, rollbackFailed);

        public IStageExecutor Stage(Action commit, Func<Task> rollback, bool rollbackFailed = false) =>
            StageInternal(commit, rollback, rollbackFailed);

        public IStageExecutor Stage(Func<Task> commit, Action rollback, bool rollbackFailed = false) =>
            StageInternal(commit, rollback, rollbackFailed);

        public IStageExecutor Stage(Func<Task> commit, Func<Task> rollback, bool rollbackFailed = false) =>
            StageInternal(commit, rollback, rollbackFailed);

        public async Task RunAsync()
        {
            var i = 0;
            var exceptions = new List<Exception>();

            // run each stage
            foreach (var (commit, _, _) in _stages)
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
            foreach (var (_, rollback, rollbackFailed) in _stages.Take(i))
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

        private StageExecutor StageInternal(Delegate commit, Delegate rollback, bool rollbackFailed)
        {
            _stages.Add((commit, rollback, rollbackFailed));

            return this;
        }
    }
}