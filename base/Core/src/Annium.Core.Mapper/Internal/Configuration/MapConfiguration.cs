using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal class MapConfiguration<S, D> : MapConfigurationBase, IMapConfiguration<S, D>
    {
        public IEnumerableFieldConfiguration<IReadOnlyDictionary<K, V>, KeyValuePair<K, V>, S, D> Field<K, V>(
            Expression<Func<D, IReadOnlyDictionary<K, V>>> fieldExpression
        ) where K : notnull
        {
            throw new NotImplementedException();
        }

        public IEnumerableFieldConfiguration<IReadOnlyCollection<F>, F, S, D> Field<F>(
            Expression<Func<D, IReadOnlyCollection<F>>> fieldExpression
        )
        {
            throw new NotImplementedException();
        }

        public IEnumerableFieldConfiguration<F[], F, S, D> Field<F>(
            Expression<Func<D, F[]>> fieldExpression
        )
        {
            throw new NotImplementedException();
        }

        public IFieldConfiguration<F, S, D> Field<F>(
            Expression<Func<D, F>> fieldExpression
        )
        {
            var cfg = new FieldConfiguration<F, S, D>(TypeHelper.ResolveProperty(fieldExpression));
            Fields.Add(cfg);

            return cfg;
        }
    }
}