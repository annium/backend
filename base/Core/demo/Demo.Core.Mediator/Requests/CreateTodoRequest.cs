namespace Demo.Core.Mediator.Requests;

internal class CreateTodoRequest : IRequest
{
    public string Value { get; }

    public CreateTodoRequest(string value)
    {
        Value = value;
    }
}