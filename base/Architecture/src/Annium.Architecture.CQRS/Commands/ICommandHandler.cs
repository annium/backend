using Annium.Core.Mediator;

namespace Annium.Architecture.CQRS.Commands
{
    public interface ICommandHandler<TRequest, TResponse> : IFinalRequestHandler<TRequest, TResponse> where TRequest : ICommand { }
}