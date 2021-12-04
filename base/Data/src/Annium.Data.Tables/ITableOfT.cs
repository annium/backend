using System;
using Annium.Core.Primitives;

namespace Annium.Data.Tables;

public interface ITable<T> : ITableSource<T>, ITableView<T>
    where T : IEquatable<T>, ICopyable<T>
{
}