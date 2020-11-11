using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper
{
    public interface IMapConfigurationBuilder<TS, T>
    {
        void With(Expression<Func<TS, T>> map);

        IMapConfigurationBuilder<TS, T> For<TF>(
            Expression<Func<T, object>> members,
            Expression<Func<TS, TF>> map
        );

        IMapConfigurationBuilder<TS, T> Ignore(
            Expression<Func<T, object>> members
        );
    }
}