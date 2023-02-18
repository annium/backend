using System.Runtime.CompilerServices;

// DisposableBox
[assembly: InternalsVisibleTo("Annium")]
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
// Mapper
[assembly: InternalsVisibleTo("Annium.Net.Types")]
// ClientWebSocket, WebSocket, WebSocketBase
[assembly: InternalsVisibleTo("Annium.Net.WebSockets")]