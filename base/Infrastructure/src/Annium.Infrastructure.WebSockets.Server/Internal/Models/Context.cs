namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal class Context<TRequest, TResponse>
    {
        public TRequest Request { get; }
        public TResponse Response { get; }

        public Context(
            TRequest request,
            TResponse response
        )
        {
            Request = request;
            Response = response;
        }
    }
}