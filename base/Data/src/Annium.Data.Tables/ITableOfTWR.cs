using System;
using Annium.Core.Primitives;

namespace Annium.Data.Tables;

public interface ITable<out TR, TW> : ITableSource<TW>, ITableView<TR>
    where TR : IEquatable<TR>, ICopyable<TR>
    where TW : notnull
{
}