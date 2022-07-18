using System;
using Annium.Extensions.Execution.Internal;
using Annium.Extensions.Execution.Internal.Background;

namespace Annium.Extensions.Execution;

public static class Executor
{
    public static IBatchExecutor Batch() => new BatchExecutor();
    public static IStageExecutor Staged() => new StageExecutor();

    public static class Background
    {
        public static IBackgroundExecutor Parallel<T>() =>
            new ParallelBackgroundExecutor<T>();

        public static IBackgroundExecutor Concurrent<T>(uint parallelism = 0) =>
            new ConcurrentBackgroundExecutor<T>(parallelism == 0u ? Environment.ProcessorCount : (int) parallelism);

        public static IBackgroundExecutor Sequential<T>() =>
            new SequentialBackgroundExecutor<T>();
    }
}