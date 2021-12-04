using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives;

namespace Annium.Data.Tables;

public static class TableSourceExtensions
{
    public static IDisposable WriteTo<T>(
        this IObservable<IChangeEvent<T>> source,
        ITableSource<T> target
    )
        where T : IEquatable<T>, ICopyable<T> =>
        source.Subscribe(target.Write);

    public static IDisposable AppendTo<T>(
        this IObservable<IChangeEvent<T>> source,
        ITableSource<T> target
    )
        where T : IEquatable<T>, ICopyable<T> =>
        source.Subscribe(target.Append);

    public static void Write<T>(
        this ITableSource<T> target,
        IChangeEvent<T> e
    )
        where T : IEquatable<T>, ICopyable<T>
    {
        switch (e)
        {
            case InitEvent<T> init:
                target.Init(init.Values.Select(x => x.Copy()).ToArray());
                break;
            case AddEvent<T> add:
                target.Set(add.Value.Copy());
                break;
            case UpdateEvent<T> update:
                target.Set(update.NewValue.Copy());
                break;
            case DeleteEvent<T> delete:
                target.Delete(delete.Value.Copy());
                break;
        }
    }

    public static void Append<T>(
        this ITableSource<T> target,
        IChangeEvent<T> e
    )
        where T : IEquatable<T>, ICopyable<T>
    {
        switch (e)
        {
            case InitEvent<T> init:
                foreach (var value in init.Values)
                    target.Set(value.Copy());
                break;
            case AddEvent<T> add:
                target.Set(add.Value.Copy());
                break;
            case UpdateEvent<T> update:
                target.Set(update.NewValue.Copy());
                break;
            case DeleteEvent<T> delete:
                target.Delete(delete.Value.Copy());
                break;
        }
    }

    public static void SyncAddDelete<T>(
        this ITableSource<T> target,
        IReadOnlyCollection<T> values
    )
        where T : IEquatable<T>, ICopyable<T>
    {
        var source = target.Source;
        var data = values.ToDictionary(target.GetKey, x => x);

        // remove unexpected values
        foreach (var (key, value) in source)
            if (!data.ContainsKey(key))
                target.Delete(value);

        // add missing values
        foreach (var (key, value) in data)
            if (!source.ContainsKey(key))
                target.Set(value);
    }

    public static void SyncAddUpdateDelete<T>(
        this ITableSource<T> target,
        IReadOnlyCollection<T> values
    )
        where T : IEquatable<T>, ICopyable<T>
    {
        var source = target.Source;
        var data = values.ToDictionary(target.GetKey, x => x);

        // remove unexpected values
        foreach (var (key, value) in source)
            if (!data.ContainsKey(key))
                target.Delete(value);

        // add or update values
        foreach (var value in values)
            target.Set(value);
    }
}