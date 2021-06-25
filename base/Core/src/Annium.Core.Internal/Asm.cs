using System.Runtime.CompilerServices;

// TypeManagerInstance, AssembliesCollector
[assembly: InternalsVisibleTo("Annium.Core.Runtime")]
// SequentialBackgroundExecutor, ParallelBackgroundExecutor
[assembly: InternalsVisibleTo("Annium.Extensions.Execution")]
// BackgroundLogScheduler
[assembly: InternalsVisibleTo("Annium.Logging.Shared")]