using System.Collections.Generic;

namespace Annium.Data.Tables
{
    public interface ITableSource<T>
        where T : notnull
    {
        void Init(IReadOnlyCollection<T> entries);
        void Set(T entry);
        void Delete(T entry);
    }
}
