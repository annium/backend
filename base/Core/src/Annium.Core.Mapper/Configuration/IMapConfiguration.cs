using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Core.Mapper
{
    public interface IMapConfiguration<S, D>
    {
        IEnumerableFieldConfiguration<IReadOnlyDictionary<K, V>, KeyValuePair<K, V>, S, D> Field<K, V>(
            Expression<Func<D, IReadOnlyDictionary<K, V>>> fieldExpression
        ) where K : notnull;
        IEnumerableFieldConfiguration<IReadOnlyCollection<F>, F, S, D> Field<F>(
            Expression<Func<D, IReadOnlyCollection<F>>> fieldExpression
        );
        IEnumerableFieldConfiguration<F[], F, S, D> Field<F>(
            Expression<Func<D, F[]>> fieldExpression
        );
        IFieldConfiguration<F, S, D> Field<F>(
            Expression<Func<D, F>> fieldExpression
        );
    }
}
