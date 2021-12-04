using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal;

internal class LogSubject<T> : ILogSubject<T>
{
    public ILogger Logger { get; }

    public LogSubject(ILogger<T> logger)
    {
        Logger = logger;
    }
}