using System;
using System.Reflection;
using Annium.Core.Reflection;

namespace Annium.Net.Types;

public static class Match
{
    public static Predicate<Type> Is(Type target) => type =>
        type == target;

    public static Predicate<Type> IsDerivedFrom(Type target) => type =>
        type.IsDerivedFrom(target);

    public static Predicate<Type> NamespaceIs(string ns) => type =>
        type.Namespace == ns;

    public static Predicate<Type> NamespaceStartsWith(string ns) => type =>
        type.Namespace?.StartsWith(ns) ?? false;

    public static Predicate<Type> IsFromAssembly(Assembly assembly) => type =>
        type.Assembly == assembly;
}