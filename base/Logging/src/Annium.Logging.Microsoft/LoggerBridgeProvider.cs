using System.Linq;
using Annium.Core.Runtime.Types;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft
{
    internal class LoggerBridgeProvider : ILoggerProvider
    {
        private readonly ILogRouter _router;
        private readonly ITypeManager _typeManager;

        public LoggerBridgeProvider(
            ILogRouter router,
            ITypeManager typeManager
        )
        {
            _router = router;
            _typeManager = typeManager;
        }

        public global::Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            var type = _typeManager.Types.FirstOrDefault(x => x.FullName == categoryName) ?? typeof(LoggerBridgeProvider);

            return new LoggerBridge(_router, type);
        }

        public void Dispose()
        {
        }
    }
}