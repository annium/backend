using System.Threading.Tasks;

namespace Annium.Logging.Shared
{
    public interface ILogHandler
    {
        ValueTask Handle(LogMessage message);
    }
}