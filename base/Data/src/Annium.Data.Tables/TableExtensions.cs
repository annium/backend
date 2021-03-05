using System;

namespace Annium.Data.Tables
{
    public static class TableExtensions
    {
        public static IDisposable WriteTo<T>(this IObservable<IChangeEvent<T>> source, ITableSource<T> target) where T : IEquatable<T> =>
            source.Subscribe(e =>
            {
                switch (e)
                {
                    case InitEvent<T> init:
                        target.Init(init.Values);
                        break;
                    case AddEvent<T> add:
                        target.Set(add.Value);
                        break;
                    case UpdateEvent<T> update:
                        target.Set(update.NewValue);
                        break;
                    case DeleteEvent<T> delete:
                        target.Delete(delete.Value);
                        break;
                }
            });

        public static IDisposable AppendTo<T>(this IObservable<IChangeEvent<T>> source, ITableSource<T> target) where T : IEquatable<T> =>
            source.Subscribe(e =>
            {
                switch (e)
                {
                    case InitEvent<T> init:
                        foreach (var value in init.Values)
                            target.Set(value);
                        break;
                    case AddEvent<T> add:
                        target.Set(add.Value);
                        break;
                    case UpdateEvent<T> update:
                        target.Set(update.NewValue);
                        break;
                    case DeleteEvent<T> delete:
                        target.Delete(delete.Value);
                        break;
                }
            });
    }
}
