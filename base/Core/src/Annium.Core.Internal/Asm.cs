using System.Runtime.CompilerServices;

// Entry
[assembly: InternalsVisibleTo("Annium.Core.Entrypoint")]
// TypeManagerInstance, AssembliesCollector
[assembly: InternalsVisibleTo("Annium.Core.Runtime")]
// SequentialBackgroundExecutor, ParallelBackgroundExecutor
[assembly: InternalsVisibleTo("Annium.Extensions.Execution")]
// StaticObservableInstance
[assembly: InternalsVisibleTo("Annium.Extensions.Reactive")]
// BackgroundLogScheduler
[assembly: InternalsVisibleTo("Annium.Logging.Shared")]