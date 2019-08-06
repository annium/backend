// using System;
// using Annium.Logging.Abstractions;
// using MEventId = Microsoft.Extensions.Logging.EventId;
// using MILogger = Microsoft.Extensions.Logging.ILogger;
// using MLogLevel = Microsoft.Extensions.Logging.LogLevel;

// namespace Annium.Logging.Abstractions
// {
//     public class LoggerBridge : MILogger
//     {
//         private readonly LoggerConfiguration configuration;

//         private readonly ILogger logger;

//         public LoggerBridge(
//             LoggerConfiguration configuration,
//             ILogger logger
//         )
//         {
//             this.configuration = configuration;
//             this.logger = logger;
//         }

//         public IDisposable BeginScope<TState>(TState state) => null;

//         public bool IsEnabled(MLogLevel logLevel)
//         {
//             var level = Map(logLevel);

//             return level >= configuration.LogLevel;
//         }

//         public void Log<TState>(
//             MLogLevel logLevel,
//             MEventId eventId,
//             TState state,
//             Exception exception,
//             Func<TState, Exception, string> formatter
//         )
//         {
//             var level = Map(logLevel);

//             if (exception == null)
//                 logger.Log(level, formatter(state, exception));
//             else
//                 logger.Error(exception);
//         }

//         private LogLevel Map(MLogLevel level)
//         {
//             switch (level)
//             {
//                 case MLogLevel.Trace:
//                     return LogLevel.Trace;
//                 case MLogLevel.Debug:
//                     return LogLevel.Debug;
//                 case MLogLevel.Information:
//                     return LogLevel.Info;
//                 case MLogLevel.Warning:
//                     return LogLevel.Warn;
//                 case MLogLevel.Error:
//                 case MLogLevel.Critical:
//                     return LogLevel.Error;
//                 default:
//                     return LogLevel.None;
//             }
//         }
//     }
// }