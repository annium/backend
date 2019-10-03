using Annium.Core.Reflection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft
{
    internal class LoggerBridgeProvider : ILoggerProvider
    {
        private readonly ILogRouter router;
        private readonly ITypeManager typeManager;

        public LoggerBridgeProvider(
            ILogRouter router,
            ITypeManager typeManager
        )
        {
            this.router = router;
            this.typeManager = typeManager;
        }

        public global::Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            var type = typeManager.GetByName(categoryName) ?? typeof(LoggerBridgeProvider);

            return new LoggerBridge(router, type);
        }

        public void Dispose() { }
    }
}