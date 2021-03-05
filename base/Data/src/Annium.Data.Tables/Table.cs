using System;
using Annium.Core.Primitives;
using Annium.Data.Tables.Internal;

namespace Annium.Data.Tables
{
    public static class Table
    {
        public static ITableBuilder<TR, TW> New<TR, TW>()
            where TR : IEquatable<TR>
            where TW : notnull
        {
            return new TableBuilder<TR, TW>();
        }

        public static ITableBuilder<T> New<T>()
            where T : IEquatable<T>, ICopyable<T>
        {
            return new TableBuilder<T>();
        }
    }
}
