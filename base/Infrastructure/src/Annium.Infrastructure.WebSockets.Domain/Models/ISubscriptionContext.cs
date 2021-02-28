using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public interface ISubscriptionContext<TInit, TMessage> : IRequestContext<TInit>
        where TInit : SubscriptionInitRequestBase
    {
        Task Handle(IStatusResult<OperationStatus> result);
        CancellationToken Token { get; }
        Task Send(TMessage message);
    }
}