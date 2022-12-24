using System;
using System.Linq.Expressions;

namespace Annium.Data.Tables;

public interface ITableBuilder<T>
    where T : IEquatable<T>
{
    ITableBuilder<T> Allow(TablePermission permissions);
    ITableBuilder<T> Key(Expression<Func<T, object>> getKey);
    ITableBuilder<T> UpdateWith(Func<T, T, bool> update);
    ITableBuilder<T> Keep(Func<T, bool> isActive);
    ITable<T> Build();
}