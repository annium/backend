using Annium.Core.Mediator;

namespace Annium.Architecture.CQRS.Queries
{
    public interface IQueryHandler<TRequest, TResponse> : IFinalRequestHandler<TRequest, TResponse> where TRequest : IQuery { }
}