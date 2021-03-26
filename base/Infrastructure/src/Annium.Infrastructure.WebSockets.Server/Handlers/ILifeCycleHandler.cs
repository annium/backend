using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers
{
    public interface ILifeCycleHandler<TState>
        where TState : ConnectionState
    {
        Task HandleStartAsync(TState state);
        Task HandleEndAsync(TState state);
    }
}