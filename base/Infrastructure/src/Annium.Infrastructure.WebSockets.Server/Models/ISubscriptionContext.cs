using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.Infrastructure.WebSockets.Server.Models;

public interface ISubscriptionContext<TInit, TMessage, TState> : IRequestContext<TInit, TState>
    where TInit : SubscriptionInitRequestBase
    where TState : ConnectionStateBase
{
    void Handle(IStatusResult<OperationStatus> result);
    void Send(TMessage message);
}