using System;

namespace Annium.Data.Tables
{
    public interface ITable<out TR, TW> : ITableSource<TW>, ITableView<TR>
        where TR : IEquatable<TR>
        where TW : notnull
    {
    }
}
