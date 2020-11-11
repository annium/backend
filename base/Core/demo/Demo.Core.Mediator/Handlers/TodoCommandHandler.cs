using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Demo.Core.Mediator.Db;
using Demo.Core.Mediator.Models;
using Demo.Core.Mediator.Requests;

namespace Demo.Core.Mediator.Handlers
{
    internal class TodoCommandHandler :
        IFinalRequestHandler<Authored<CreateTodoRequest>, int>,
        IFinalRequestHandler<DeleteTodoRequest, bool>
    {
        private readonly TodoRepository _todoRepository;

        public TodoCommandHandler(
            TodoRepository todoRepository
        )
        {
            _todoRepository = todoRepository;
        }

        public Task<int> HandleAsync(
            Authored<CreateTodoRequest> request,
            CancellationToken ct
        )
        {
            return Task.FromResult(_todoRepository.Add(new Todo(request.Entity.Value)));
        }

        public Task<bool> HandleAsync(
            DeleteTodoRequest request,
            CancellationToken ct
        )
        {
            return Task.FromResult(_todoRepository.Delete(request.Id));
        }
    }
}