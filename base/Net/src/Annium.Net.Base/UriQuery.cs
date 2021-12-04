using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Base;

public sealed record UriQuery : IDictionary<string, StringValues>, IReadOnlyDictionary<string, StringValues>,
    ICopyable<UriQuery>
{
    public static UriQuery New()
    {
        return new(new Dictionary<string, StringValues>());
    }

    public static UriQuery Parse(string query)
    {
        var data = QueryHelpers.ParseQuery(query);

        return new UriQuery(data);
    }

    private readonly IDictionary<string, StringValues> _data;

    private UriQuery(IDictionary<string, StringValues> data)
    {
        _data = data;
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
    public void Add(KeyValuePair<string, StringValues> item) => _data.Add(item);
    public void Clear() => _data.Clear();
    public bool Contains(KeyValuePair<string, StringValues> item) => _data.Contains(item);
    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex) => _data.Clear();
    public bool Remove(KeyValuePair<string, StringValues> item) => _data.Remove(item);
    int ICollection<KeyValuePair<string, StringValues>>.Count => _data.Count;
    public bool IsReadOnly => false;
    public void Add(string key, StringValues value) => _data.Add(key, value);
    bool IDictionary<string, StringValues>.ContainsKey(string key) => _data.ContainsKey(key);

    bool IReadOnlyDictionary<string, StringValues>.TryGetValue(string key, out StringValues value) =>
        _data.TryGetValue(key, out value);

    public bool Remove(string key) => _data.Remove(key);
    bool IReadOnlyDictionary<string, StringValues>.ContainsKey(string key) => _data.ContainsKey(key);

    bool IDictionary<string, StringValues>.TryGetValue(string key, out StringValues value) =>
        _data.TryGetValue(key, out value);

    IEnumerable<string> IReadOnlyDictionary<string, StringValues>.Keys => _data.Keys;
    IEnumerable<StringValues> IReadOnlyDictionary<string, StringValues>.Values => _data.Values;
    ICollection<string> IDictionary<string, StringValues>.Keys => _data.Keys;
    ICollection<StringValues> IDictionary<string, StringValues>.Values => _data.Values;
    int IReadOnlyCollection<KeyValuePair<string, StringValues>>.Count => _data.Count;

    public StringValues this[string key]
    {
        get => _data[key];
        set => _data[key] = value;
    }

    public bool Equals(UriQuery? other) => other is not null && GetHashCode() == other.GetHashCode();

    public UriQuery Copy() => new(_data.ToDictionary(x => x.Key, x => x.Value));

    public override int GetHashCode()
    {
        var code = 0;

        foreach (var (key, value) in _data.ToArray())
            code ^= HashCode.Combine(key, HashCodeSeq.Combine(value));

        return code;
    }

    public override string ToString() => _data.Count > 0
        ? '?' + string.Join("&", _data.Select(x => string.Join("&", x.Value.Select(y => $"{x.Key}={y}"))))
        : string.Empty;
}