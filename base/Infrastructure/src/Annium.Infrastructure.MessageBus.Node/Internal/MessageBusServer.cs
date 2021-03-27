using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class MessageBusServer : IMessageBusServer
    {
        private readonly IMessageBusNode _node;

        public MessageBusServer(
            IMessageBusNode node
        )
        {
            _node = node;
        }

        public IDisposable Handle<TRequest, TResponse>(
            string topic,
            Func<IMessageBusRequestContext<TRequest, TResponse>, Task> process
        )
        {
            var (requestTopic, responseTopic) = Helper.GetRequestResponseTopics(topic);

            return _node
                .Listen<MessageBusEnvelope<TRequest>>(requestTopic)
                .Subscribe(async x =>
                {
                    try
                    {
                        var ctx = new MessageBusRequestContext<TRequest, TResponse>(x.Data);
                        await process(ctx);
                        if (ctx.IsResponded)
                            await _node.Send(responseTopic, MessageBusEnvelope.Data(x.Id, ctx.Response));
                    }
                    catch (Exception e)
                    {
                        await _node.Send(responseTopic, MessageBusEnvelope.Error(x.Id, e.Message));
                    }
                });
        }
    }
}
