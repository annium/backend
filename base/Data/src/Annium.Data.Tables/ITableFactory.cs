using System;

namespace Annium.Data.Tables;

public interface ITableFactory
{
    ITableBuilder<T> New<T>()
        where T : IEquatable<T>;
}