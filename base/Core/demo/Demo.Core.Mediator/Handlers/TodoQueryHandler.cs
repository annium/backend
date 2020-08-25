using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Demo.Core.Mediator.Db;
using Demo.Core.Mediator.Models;
using Demo.Core.Mediator.Requests;

namespace Demo.Core.Mediator.Handlers
{
    internal class TodoQueryHandler // : IRequestHandler<ListTodosRequest, Todo[]>
    {
        private readonly TodoRepository todoRepository;

        public TodoQueryHandler(
            TodoRepository todoRepository
        )
        {
            this.todoRepository = todoRepository;
        }

        public Task<Todo[]> HandleAsync(
            ListTodosRequest request,
            CancellationToken cancellationToken,
            Func<Task<Todo[]>> next
        )
        {
            return Task.FromResult(todoRepository.GetAll().ToArray());
        }
    }
}