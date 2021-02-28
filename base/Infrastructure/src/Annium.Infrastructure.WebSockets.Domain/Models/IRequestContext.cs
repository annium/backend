namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public interface IRequestContext<TRequest>
    {
        TRequest Request { get; }
        IConnectionState State { get; }
        void Deconstruct(out TRequest request, out IConnectionState state);
    }
}