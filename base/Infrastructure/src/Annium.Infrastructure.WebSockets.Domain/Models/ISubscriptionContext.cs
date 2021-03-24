using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public interface ISubscriptionContext<TInit, TMessage> : IRequestContext<TInit>
        where TInit : SubscriptionInitRequestBase
    {
        void Handle(IStatusResult<OperationStatus> result);
        void Send(TMessage message);
    }
}