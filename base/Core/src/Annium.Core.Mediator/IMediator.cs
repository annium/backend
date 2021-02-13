using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Mediator
{
    public interface IMediator
    {
        Task<TResponse> SendAsync<TResponse>(
            object request,
            CancellationToken cancellationToken = default
        );

        Task<TResponse> SendAsync<TResponse>(
            IServiceProvider serviceProvider,
            object request,
            CancellationToken cancellationToken = default
        );
    }
}