using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution
{
    public interface IStageExecutor
    {
        IStageExecutor Stage(Action commit, Action rollback, bool rollbackFailed = false);
        IStageExecutor Stage(Action commit, Func<Task> rollback, bool rollbackFailed = false);
        IStageExecutor Stage(Func<Task> commit, Action rollback, bool rollbackFailed = false);
        IStageExecutor Stage(Func<Task> commit, Func<Task> rollback, bool rollbackFailed = false);
    }
}