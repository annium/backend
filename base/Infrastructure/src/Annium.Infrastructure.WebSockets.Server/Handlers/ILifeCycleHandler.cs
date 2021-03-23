using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers
{
    public interface ILifeCycleHandler
    {
        Task HandleStartAsync(IConnectionState state);
        Task HandleEndAsync(IConnectionState state);
    }
}