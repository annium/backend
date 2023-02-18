using System;
using Annium.Core.Reflection;

namespace Annium.Net.Types;

public static class Match
{
    public static Predicate<Type> Is(Type target) => type =>
        type == target;

    public static Predicate<Type> IsDerivedFrom(Type target) => type =>
        type.IsDerivedFrom(target);
}