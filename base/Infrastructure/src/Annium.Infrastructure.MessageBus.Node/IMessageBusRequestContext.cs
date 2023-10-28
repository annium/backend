namespace Annium.Infrastructure.MessageBus.Node;

public interface IMessageBusRequestContext<TRequest, TResponse>
{
    TRequest Request { get; }
    bool IsResponded { get; }
    void WriteResponse(TResponse response);
}
