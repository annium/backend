using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Mediator
{
    public interface IMediator
    {
        Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default
        );
    }
}