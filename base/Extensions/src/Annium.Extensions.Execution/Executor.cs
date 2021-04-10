using Annium.Extensions.Execution.Internal;

namespace Annium.Extensions.Execution
{
    public static class Executor
    {
        public static IBatchExecutor Batch() => new BatchExecutor();
        public static IStageExecutor Staged() => new StageExecutor();
    }
}