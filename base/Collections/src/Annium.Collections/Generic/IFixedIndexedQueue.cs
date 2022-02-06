using System.Collections.Generic;

namespace Annium.Collections.Generic;

public interface IFixedIndexedQueue<T> : IReadOnlyList<T>
{
    void Add(T item);
}