using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Core.Primitives;
using Annium.Net.Types.Extensions;

namespace Annium.Net.Types.Models;

public sealed record Namespace : IEnumerable<string>
{
    #region static

    public static Namespace New(IEnumerable<string> ns) => new(ns.ToArray().EnsureValidNamespace());
    public static Namespace New(IReadOnlyList<string> ns) => new(ns.EnsureValidNamespace());

    #endregion

    #region instance

    private readonly IReadOnlyList<string> _parts;

    private Namespace(IReadOnlyList<string> parts)
    {
        _parts = parts;
    }

    public bool StartsWith(Namespace ns)
    {
        if (ns._parts.Count > _parts.Count)
            return false;

        for (var i = 0; i < ns._parts.Count; i++)
            if (_parts[i] != ns._parts[i])
                return false;

        return true;
    }

    public Namespace From(Namespace ns)
    {
        if (!StartsWith(ns))
            throw new ArgumentException($"Namespace {this} doesn't contain namespace {ns}");

        return new Namespace(_parts.Skip(ns._parts.Count).ToArray());
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<string> GetEnumerator() => _parts.GetEnumerator();

    public string ToPath(string basePath) => Path.Combine(basePath, Path.Combine(_parts.ToArray()));

    public override string ToString() => _parts.ToNamespaceString();

    public bool Equals(Namespace? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _parts.SequenceEqual(other._parts);
    }

    public override int GetHashCode() => HashCodeSeq.Combine(_parts);

    #endregion
}