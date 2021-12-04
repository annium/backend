namespace Demo.Core.Mediator.Models;

internal class Todo
{
    public int Id { get; }
    public string Value { get; }

    public Todo(int id, string value) : this(value)
    {
        Id = id;
    }

    public Todo(string value)
    {
        Value = value;
    }
}