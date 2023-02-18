using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Container;

internal class Index<TKey, TValue> : IIndex<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public int Count => _data.Count;
    public IEnumerable<TKey> Keys => _data.Select(x => x.Key).Distinct();
    public IEnumerable<TValue> Values => _data.Select(x => x.Value).Distinct();

    public TValue this[TKey key]
    {
        get
        {
            var values = _data.Where(x => x.Key.Equals(key)).Select(x => x.Value).ToArray();

            return values.Length switch
            {
                0 => throw new InvalidOperationException(
                    $"No {typeof(TValue).FriendlyName()} value registered for key {key} of type {typeof(TKey).FriendlyName()}"
                ),
                > 1 => throw new InvalidOperationException(
                    $"Ambiguous match of {values.Length} {typeof(TValue).FriendlyName()} values registered for key {key} of type {typeof(TKey).FriendlyName()}"
                ),
                _ => values[0]
            };
        }
    }

    private readonly IReadOnlyCollection<KeyValuePair<TKey, TValue>> _data;

    public Index(IEnumerable<KeyValue<TKey, TValue>> data)
    {
        _data = data.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value)).ToArray();
    }

    public bool ContainsKey(TKey key) =>
        TryGetValue(key, out _);

    public bool TryGetValue(TKey key, out TValue value)
    {
        var values = _data.Where(x => x.Key.Equals(key)).Select(x => x.Value).ToArray();
        value = default!;

        switch (values.Length)
        {
            case 0:
            case > 1:
                return false;
            default:
                value = values[0];

                return true;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
}