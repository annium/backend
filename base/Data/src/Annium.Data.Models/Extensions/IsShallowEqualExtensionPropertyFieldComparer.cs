using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Reflection;

namespace Annium.Data.Models.Extensions
{
    public static partial class ShallowEqualPlainExtensions
    {
        private static LambdaExpression BuildPropertyFieldComparer(Type type)
        {
            var a = Expression.Parameter(type);
            var b = Expression.Parameter(type);
            var parameters = new List<ParameterExpression> { a, b };

            // if IEquatable is implemented - return it's call
            var equals = type.GetMethods().FirstOrDefault(x =>
                x.IsPublic &&
                !x.IsStatic &&
                x.Name == nameof(Equals) &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == type
            );
            if (equals != null)
                return Expression.Lambda(Expression.Call(a, equals, b), a, b);

            // if Equals is overriden - return it's call
            equals = type.GetMethods().FirstOrDefault(x =>
                x.IsPublic &&
                !x.IsStatic &&
                x.DeclaringType == type &&
                x.Name == nameof(Equals) &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == typeof(object)
            );
            if (equals != null)
                return Expression.Lambda(Expression.Call(a, equals, Expression.Convert(b, typeof(object))), a, b);

            var returnTarget = Expression.Label(typeof(bool));

            var vars = new List<ParameterExpression>();
            var expressions = new List<Expression>();
            if (type.IsClass)
            {
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
            }

            var propertyExpressions = type
                .GetAllProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead)
                .ToArray();
            foreach (var property in propertyExpressions)
            {
                var comparer = ResolveComparer(property.PropertyType);
                var comparerVar = Expression.Variable(comparer.Type);
                var comparerInit = Expression.Assign(comparerVar, comparer);
                vars.Add(comparerVar);
                expressions.Add(comparerInit);
                var ax = Expression.Property(a, property);
                var bx = Expression.Property(b, property);

                expressions.Add(Expression.IfThen(
                    Expression.Not(Expression.Invoke(comparerVar, ax, bx)),
                    Expression.Return(returnTarget, Expression.Constant(false))
                ));
            }

            var fieldExpressions = type
                .GetAllFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fieldExpressions)
            {
                var comparerVar = Expression.Variable(typeof(Delegate));
                var comparerInit = Expression.Assign(comparerVar, ResolveComparer(field.FieldType));
                vars.Add(comparerVar);
                expressions.Add(comparerInit);
                var ax = Expression.Field(a, field);
                var bx = Expression.Field(b, field);

                expressions.Add(Expression.IfThen(
                    Expression.Not(Expression.Invoke(comparerVar, ax, bx)),
                    Expression.Return(returnTarget, Expression.Constant(false))
                ));
            }

            expressions.Add(Expression.Label(returnTarget, Expression.Constant(true)));

            return Expression.Lambda(Expression.Block(vars, expressions), parameters);
        }
    }
}