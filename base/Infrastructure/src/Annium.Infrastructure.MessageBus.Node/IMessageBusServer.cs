using System;
using System.Threading.Tasks;

namespace Annium.Infrastructure.MessageBus.Node
{
    public interface IMessageBusServer
    {
        IDisposable Handle<TRequest, TResponse>(string topic, Func<IMessageBusRequestContext<TRequest, TResponse>, Task> process);
    }
}
