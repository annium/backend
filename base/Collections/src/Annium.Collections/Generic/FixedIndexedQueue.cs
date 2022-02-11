using System;
using System.Collections;
using System.Collections.Generic;

namespace Annium.Collections.Generic;

public class FixedIndexedQueue<T> : IFixedIndexedQueue<T>
{
    public int Count => _count;
    private readonly int _capacity;
    private readonly int _maxIndex;
    private readonly T[] _data;
    private int _index;
    private int _count;

    public FixedIndexedQueue(int capacity)
    {
        _capacity = capacity;
        _maxIndex = capacity - 1;
        _data = new T[capacity];
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index > _maxIndex)
                throw new ArgumentOutOfRangeException($"Index {index} is out of range [0;{_maxIndex}]");

            return _data[(_index + index) % _capacity];
        }
    }

    public void Add(T item)
    {
        if (_count < _capacity)
        {
            _data[_count] = item;
            _count++;
        }
        else
        {
            _data[_index] = item;
            _index = _index == _maxIndex ? 0 : _index + 1;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < _count; i++)
            yield return _data[(_index + i) % _capacity];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public interface IFixedIndexedQueue<T> : IReadOnlyList<T>
{
    void Add(T item);
}