namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public interface IRequestContext<TRequest, TState>
        where TState : ConnectionState
    {
        TRequest Request { get; }
        TState State { get; }
        void Deconstruct(out TRequest request, out TState state);
    }
}