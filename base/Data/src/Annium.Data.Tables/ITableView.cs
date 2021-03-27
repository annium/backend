using System;
using System.Collections.Generic;
using Annium.Core.Primitives;

namespace Annium.Data.Tables
{
    public interface ITableView<out T> : IReadOnlyCollection<T>, IObservable<IChangeEvent<T>>, IDisposable
        where T : IEquatable<T>, ICopyable<T>
    {
    }
}
