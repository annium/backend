using System;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class MessageBusRequestContext<TRequest, TResponse> : IMessageBusRequestContext<TRequest, TResponse>
    {
        public TRequest Request { get; }
        public bool IsResponded { get; private set; }
        internal TResponse Response { get; private set; } = default!;

        public MessageBusRequestContext(TRequest request)
        {
            Request = request;
        }

        public void WriteResponse(TResponse response)
        {
            if (IsResponded)
                throw new InvalidOperationException("Response already written");

            IsResponded = true;
            Response = response;
        }
    }
}
