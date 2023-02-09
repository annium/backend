using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Extensions;

public static class NamespaceExtensions
{
    public static Namespace GetNamespace(this ContextualType type) => type.Type.GetNamespace();
    public static Namespace GetNamespace(this Type type) => type.Namespace!.ToNamespace();
    public static Namespace ToNamespace(this string ns) => Namespace.New(ns.ToNamespaceArray());
    public static Namespace ToNamespace(this IEnumerable<string> ns) => Namespace.New(ns);

    public static string[] ToNamespaceArray(this string ns) => ns switch
    {
        "" => Array.Empty<string>(),
        _  => ns.Split('.').ToArray().EnsureValidNamespace()
    };

    public static string ToNamespaceString(this IEnumerable<string> ns) => string.Join('.', ns);

    internal static T EnsureValidNamespace<T>(this T ns)
        where T : IEnumerable<string>
    {
        if (ns.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException($"Namespace {ns.ToNamespaceString()} contains empty parts");

        return ns;
    }
}