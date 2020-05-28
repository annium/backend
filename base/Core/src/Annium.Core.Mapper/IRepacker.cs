using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper
{
    public interface IRepacker
    {
        Func<Expression, Expression> Repack(Expression ex);
    }
}