using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Data.Models.Extensions
{
    public static partial class ShallowEqualPlainExtensions
    {
        private static LambdaExpression BuildKeyedCollectionComparer(
            Type type,
            Type keyType,
            Type valueType,
            Func<ParameterExpression, Expression> getCount,
            Func<ParameterExpression, Expression> getKeys,
            Func<ParameterExpression, ParameterExpression, Expression> containsKey,
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

            var keyEnumerableType = typeof(IEnumerable<>).MakeGenericType(keyType);
            var keyEnumeratorType = typeof(IEnumerator<>).MakeGenericType(keyType);

            var enumeratorVar = Expression.Variable(keyEnumeratorType);
            vars.Add(enumeratorVar);

            var getEnumerator = keyEnumerableType.GetMethod(nameof(IEnumerable<object>.GetEnumerator))!;
            expressions.Add(Expression.Assign(enumeratorVar, Expression.Call(getKeys(a), getEnumerator)));

            var aKeyVar = Expression.Variable(keyType);
            var bKeyVar = Expression.Variable(keyType);

            var moveNext = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))!;
            var current = keyEnumeratorType.GetProperty(nameof(IEnumerator<object>.Current))!;
            var firstOrDefault = typeof(Enumerable).GetMethods()
                .Single(x => x.Name == nameof(Enumerable.FirstOrDefault) && x.GetParameters().Length == 2)
                .MakeGenericMethod(keyType);

            var keyComparerVar = Expression.Variable(typeof(Func<,,>).MakeGenericType(keyType, keyType, typeof(bool)));
            vars.Add(keyComparerVar);

            expressions.Add(Expression.Assign(keyComparerVar, ResolveComparer(keyType)));

            var valueComparerVar = Expression.Variable(typeof(Func<,,>).MakeGenericType(valueType, valueType, typeof(bool)));
            vars.Add(valueComparerVar);

            expressions.Add(Expression.Assign(valueComparerVar, ResolveComparer(valueType)));
            var keyFinderParam = Expression.Parameter(keyType);

            var breakLabel = Expression.Label(typeof(void));
            expressions.Add(Expression.Loop(
                Expression.Block(
                    new[] { aKeyVar, bKeyVar },
                    Expression.IfThen(
                        Expression.Not(Expression.Call(enumeratorVar, moveNext)),
                        Expression.Break(breakLabel)
                    ),
                    Expression.Assign(aKeyVar, Expression.Property(enumeratorVar, current)),
                    Expression.Assign(bKeyVar, Expression.Call(
                        firstOrDefault,
                        getKeys(b),
                        Expression.Lambda(Expression.Invoke(keyComparerVar, keyFinderParam, aKeyVar), keyFinderParam)
                    )),
                    Expression.IfThen(
                        Expression.AndAlso(Expression.Not(containsKey(b, aKeyVar)), Expression.Equal(bKeyVar, Expression.Constant(null))),
                        Expression.Return(returnTarget, Expression.Constant(false))
                    ),
                    Expression.IfThen(
                        Expression.Not(Expression.Invoke(
                            valueComparerVar,
                            getElement(a, aKeyVar),
                            getElement(b, bKeyVar)
                        )),
                        Expression.Return(returnTarget, Expression.Constant(false))
                    )
                ),
                breakLabel
            ));

            expressions.Add(Expression.Label(returnTarget, Expression.Constant(true)));

            return Expression.Lambda(Expression.Block(vars, expressions), parameters);
        }
    }
}