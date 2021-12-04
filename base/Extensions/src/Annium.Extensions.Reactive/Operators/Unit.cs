using System.Reactive;
using System.Reactive.Linq;

namespace System;

public static class UnitOperatorExtensions
{
    public static IObservable<Unit> Unit<TSource>(
        this IObservable<TSource> source
    ) => source.Select(_ => Reactive.Unit.Default);
}