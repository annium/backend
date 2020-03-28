using System;
using System.Linq.Expressions;

namespace Annium.Data.Models.Extensions
{
    public static partial class IsShallowEqualExtensions
    {
        private static LambdaExpression BuildExtensionCallComparer(Type type)
        {
            var a = Expression.Parameter(type);
            var b = Expression.Parameter(type);

            var method = typeof(IsShallowEqualExtensions).GetMethod(nameof(IsShallowEqual))!.MakeGenericMethod(type, type);

            return Expression.Lambda(Expression.Call(null, method, a, b), a, b);
        }
    }
}