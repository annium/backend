namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public interface IRequestContext<TRequest, TState>
        where TState : ConnectionStateBase
    {
        TRequest Request { get; }
        TState State { get; }
        void Deconstruct(out TRequest request, out TState state);
    }
}