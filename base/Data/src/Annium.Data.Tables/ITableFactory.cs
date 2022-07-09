using System;
using Annium.Core.Primitives;

namespace Annium.Data.Tables;

public interface ITableFactory
{
    ITableBuilder<TR, TW> New<TR, TW>()
        where TR : IEquatable<TR>, ICopyable<TR>
        where TW : notnull;

    ITableBuilder<T> New<T>()
        where T : IEquatable<T>, ICopyable<T>;
}