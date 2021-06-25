using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Logging.Shared
{
    public interface IAsyncLogHandler
    {
        ValueTask Handle(IReadOnlyCollection<LogMessage> messages);
    }
}