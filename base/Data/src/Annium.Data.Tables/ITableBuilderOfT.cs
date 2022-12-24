using System;
using System.Linq.Expressions;
using Annium.Core.Primitives;

namespace Annium.Data.Tables;

public interface ITableBuilder<T>
    where T : IEquatable<T>, ICopyable<T>
{
    ITableBuilder<T> Allow(TablePermission permissions);
    ITableBuilder<T> Key(Expression<Func<T, object>> getKey);
    ITableBuilder<T> UpdateWith(Action<T, T> update);
    ITableBuilder<T> Keep(Func<T, bool> isActive);
    ITable<T> Build();
}