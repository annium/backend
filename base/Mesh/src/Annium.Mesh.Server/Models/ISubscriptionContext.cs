using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Server.Models;

public interface ISubscriptionContext<TInit, TMessage> : IRequestContext<TInit>
{
    void Handle(IStatusResult<OperationStatus> result);
    void Send(TMessage message);
}
