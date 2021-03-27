using System;
using System.Linq;
using Annium.Core.Primitives;

namespace Annium.Data.Tables
{
    public static class TableExtensions
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
    }
}