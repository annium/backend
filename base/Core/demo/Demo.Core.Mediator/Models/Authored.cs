namespace Demo.Core.Mediator.Models;

internal class Authored<T>
{
    public int AuthorId { get; }
    public T Entity { get; }

    internal Authored(
        int authorId,
        T entity
    )
    {
        AuthorId = authorId;
        Entity = entity;
    }
}