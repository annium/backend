using System;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Extensions.Mapper
{
    // Summary:
    //      Repacks given expression with given source expression, replacing parameter expressions to given source expression
    internal class Repacker
    {
        public Func<Expression, Expression> Repack(Expression ex) => (Expression source) =>
        {
            if (ex == null)
                return null;

            switch (ex)
            {
                case LambdaExpression lambda:
                    return Lambda(lambda) (source);
                case MemberExpression member:
                    return Member(member) (source);
                case MethodCallExpression call:
                    return Call(call) (source);
                case ParameterExpression param:
                    return source;
                default:
                    throw new InvalidOperationException($"Can't repack {ex.NodeType} expression");
            }
        };

        private Func<Expression, Expression> Lambda(LambdaExpression ex) => (Expression source) =>
            Expression.Lambda(Repack(ex.Body) (source), new [] { source as ParameterExpression });

        private Func<Expression, Expression> Member(MemberExpression ex) => (Expression source) =>
            Expression.MakeMemberAccess(Repack(ex.Expression) (source), ex.Member);

        private Func<Expression, Expression> Call(MethodCallExpression ex) => (Expression source) =>
            Expression.Call(Repack(ex.Object) (source), ex.Method, ex.Arguments.Select(a => Repack(a) (source)).ToArray());
    }
}