using System.Runtime.CompilerServices;

// Entry
[assembly: InternalsVisibleTo("Annium.Core.Entrypoint")]
// DisposableBox
[assembly: InternalsVisibleTo("Annium.Core.Primitives")]
// TypeManagerInstance, AssembliesCollector
[assembly: InternalsVisibleTo("Annium.Core.Runtime")]
// SequentialBackgroundExecutor, ParallelBackgroundExecutor
[assembly: InternalsVisibleTo("Annium.Extensions.Execution")]
// StaticObservableInstance
[assembly: InternalsVisibleTo("Annium.Extensions.Reactive")]
// BackgroundLogScheduler
[assembly: InternalsVisibleTo("Annium.Logging.Shared")]