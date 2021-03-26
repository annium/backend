using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public interface ISubscriptionContext<TInit, TMessage, TState> : IRequestContext<TInit, TState>
        where TInit : SubscriptionInitRequestBase
        where TState : ConnectionState
    {
        void Handle(IStatusResult<OperationStatus> result);
        void Send(TMessage message);
    }
}