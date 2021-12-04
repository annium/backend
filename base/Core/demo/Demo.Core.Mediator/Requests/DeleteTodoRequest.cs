namespace Demo.Core.Mediator.Requests;

internal class DeleteTodoRequest : IRequest
{
    public int Id { get; }

    public DeleteTodoRequest(int id)
    {
        Id = id;
    }
}