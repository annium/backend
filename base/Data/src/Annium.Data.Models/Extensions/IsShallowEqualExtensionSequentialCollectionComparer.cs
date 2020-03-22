using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Data.Models.Extensions
{
    public static partial class ShallowEqualPlainExtensions
    {
        private static LambdaExpression BuildSequentialCollectionComparer(
            Type type,
            Type elementType,
            Func<ParameterExpression, Expression> getCount,
            Func<ParameterExpression, ParameterExpression, Expression> getElement
        )
        {
            var a = Expression.Parameter(type);
            var b = Expression.Parameter(type);
            var parameters = new[] { a, b };

            var returnTarget = Expression.Label(typeof(bool));

            var vars = new List<ParameterExpression>();
            var expressions = new List<Expression>();

            var countVar = Expression.Variable(typeof(int));
            vars.Add(countVar);

            expressions.Add(Expression.IfThen(
                Expression.NotEqual(
                    Expression.ReferenceEqual(a, Expression.Constant(null)),
                    Expression.ReferenceEqual(b, Expression.Constant(null))
                ),
                Expression.Return(returnTarget, Expression.Constant(false))
            ));

            expressions.Add(Expression.IfThen(
                Expression.ReferenceEqual(a, b),
                Expression.Return(returnTarget, Expression.Constant(true))
            ));

            expressions.Add(Expression.Assign(countVar, getCount(a)));
            expressions.Add(Expression.IfThen(
                Expression.NotEqual(countVar, getCount(b)),
                Expression.Return(returnTarget, Expression.Constant(false))
            ));

            var comparerVar = Expression.Variable(typeof(Func<,,>).MakeGenericType(elementType, elementType, typeof(bool)));
            vars.Add(comparerVar);

            expressions.Add(Expression.Assign(comparerVar, ResolveComparer(elementType)));

            var loopVar = Expression.Variable(typeof(int));
            var breakLabel = Expression.Label(typeof(void));
            expressions.Add(Expression.Block(
                new[] { loopVar },
                Expression.Assign(loopVar, Expression.Constant(0)),
                Expression.Loop(
                    Expression.Block(
                        Expression.IfThen(
                            Expression.Not(Expression.LessThan(loopVar, countVar)),
                            Expression.Break(breakLabel)
                        ),
                        Expression.IfThen(
                            Expression.Not(Expression.Invoke(
                                comparerVar,
                                getElement(a, loopVar),
                                getElement(b, loopVar)
                            )),
                            Expression.Return(returnTarget, Expression.Constant(false))
                        ),
                        Expression.AddAssign(loopVar, Expression.Constant(1))
                    ),
                    breakLabel
                )
            ));

            expressions.Add(Expression.Label(returnTarget, Expression.Constant(true)));

            return Expression.Lambda(Expression.Block(vars, expressions), parameters);
        }
    }
}