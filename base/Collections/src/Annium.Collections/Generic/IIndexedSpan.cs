namespace Annium.Collections.Generic;

public interface IIndexedSpan<out T> : IReadOnlyIndexedSpan<T>
{
    bool Move(int offset);
}