using System;
using System.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal class FieldConfiguration<F, S, D> : FieldConfigurationBase, IFieldConfiguration<F, S, D>
    {
        public FieldConfiguration(
            PropertyInfo property
        ) : base(property)
        {
        }

        public IFieldConfiguration<F, S, D> When(Func<S, F, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IFieldConfiguration<F, S, D> When(Func<F, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IMapConfiguration<S, D> With(Func<IMappingContext, S, F> map)
        {
            throw new NotImplementedException();
        }

        public IMapConfiguration<S, D> With(Func<S, F> map)
        {
            throw new NotImplementedException();
        }

        public IMapConfiguration<S, D> Ignore()
        {
            throw new NotImplementedException();
        }

        public IMapConfiguration<S, D> Throw<E>(Func<S, F, E> buildException) where E : Exception
        {
            throw new NotImplementedException();
        }

        public IMapConfiguration<S, D> Throw<E>(Func<F, E> buildException) where E : Exception
        {
            throw new NotImplementedException();
        }

        public IMapConfiguration<S, D> Throw<E>(Func<E> buildException) where E : Exception
        {
            throw new NotImplementedException();
        }
    }
}