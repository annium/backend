using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Data.Models.Extensions
{
    public static partial class IsShallowEqualExtensions
    {
        private static MethodInfo? ResolveEqualsMethod(Type type)
        {
            var methods = type.GetMethods()
                .Where(x =>
                    x.IsPublic &&
                    !x.IsStatic &&
                    x.Name == nameof(Equals) &&
                    x.GetParameters().Length == 1
                )
                .ToArray();

            var equals = methods.SingleOrDefault(x => x.GetParameters()[0].ParameterType == type);
            if (equals != null)
                return equals;

            return methods.SingleOrDefault(x => x.DeclaringType == type && x.GetParameters()[0].ParameterType == typeof(object));
        }

        private static LambdaExpression BuildEqualsComparer(Type type, MethodInfo equalsMethod)
        {
            var a = Expression.Parameter(type);
            var b = Expression.Parameter(type);
            var parameters = new List<ParameterExpression> { a, b };

            var returnTarget = Expression.Label(typeof(bool));

            var vars = new List<ParameterExpression>();
            var expressions = new List<Expression>();

            // if (type.IsClass)
            //     expressions.AddRange(AddReferenceEqualityChecks(a, b, returnTarget));

            var equalsExpression = equalsMethod.GetParameters()[0].ParameterType == type
                ? Expression.Call(a, equalsMethod, b)
                : Expression.Call(a, equalsMethod, Expression.Convert(b, equalsMethod.GetParameters()[0].ParameterType));

            expressions.Add(Expression.Label(returnTarget, equalsExpression));

            return Expression.Lambda(Expression.Block(vars, expressions), parameters);
        }
    }
}