using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class MessageBusClient : IMessageBusClient
    {
        private readonly IMessageBusNode _node;

        public MessageBusClient(
            IMessageBusNode node
        )
        {
            _node = node;
        }

        public async Task<IResult<T>> Fetch<T>(string topic, object request, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();

            var tcs = new TaskCompletionSource<IResult<T>>();
            ct.Register(tcs.SetCanceled);
            _node.Listen<MessageBusEnvelope<T>>()
                .Where(x => x.Id == id)
                .Subscribe(
                    x =>
                    {
                        if (x.IsSuccess)
                            tcs.SetResult(Result.New(x.Data));
                        else
                            tcs.SetException(new Exception(x.Error));
                    },
                    tcs.SetException,
                    ct
                );

            await _node.Send(MessageBusEnvelope.Data(id, request));

            var result = await tcs.Task;

            return result;
        }
    }
}