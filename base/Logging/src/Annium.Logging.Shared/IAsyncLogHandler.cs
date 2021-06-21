using System.Threading.Tasks;

namespace Annium.Logging.Shared
{
    public interface IAsyncLogHandler
    {
        ValueTask Handle(LogMessage message);
    }
}