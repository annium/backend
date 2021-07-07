using System.Threading.Tasks;

namespace Annium.Infrastructure.WebSockets.Server.Models
{
    public interface IValueLoader<TConfig, TValue>
    {
        ValueTask<TValue> LoadAsync(TConfig config);
    }

    public interface IValueLoader<TValue>
    {
        ValueTask<TValue> LoadAsync();
    }
}