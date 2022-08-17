using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal;

internal class LogSubject<T> : ILogSubject<T>
    where T : class
{
    public ILogger<T> Logger { get; }

    public LogSubject(ILogger<T> logger)
    {
        Logger = logger;
    }
}